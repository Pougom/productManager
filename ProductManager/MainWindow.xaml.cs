using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Data.SQLite;

namespace ProductManager
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Product> _products = new ObservableCollection<Product>();

        public MainWindow()
        {
            InitializeComponent();
            CreateDatabase();
            LoadProductsFromDatabase();
            ProductList.ItemsSource = _products;
        }

        private void CreateDatabase()
        {
            try
            {
                //wenn der Block fertig ist, wird die Verbíndung automatisch geschlossen
                using (var connection = new SQLiteConnection("Data Source=products.db;Version=3;"))
                {
                    connection.Open();  //Offne die verbindung zur Datenbank

                    const string sql = @"
                        CREATE TABLE IF NOT EXISTS Product (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Price REAL NOT NULL,
                            Category TEXT NOT NULL
                        );
                    ";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();   // SQL-Befehl ausführen
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Erstellen der Datenbank:\n" + ex.Message);
            }
        }

        private void LoadProductsFromDatabase()
        {
            try
            {
                _products.Clear();

                using (var connection = new SQLiteConnection("Data Source=products.db;Version=3;"))
                {
                    connection.Open();

                    const string sql = "SELECT Id, Name, Price, Category FROM Product";

                    using (var command = new SQLiteCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _products.Add(new Product
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDouble(2),
                                Category = reader.GetString(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Daten:\n" + ex.Message);
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Product P = new Product
                {
                    Name = NameBox.Text,
                    Price = double.Parse(PriceBox.Text),
                    Category = CategoryBox.Text
                };

                using (var connection = new SQLiteConnection("Data Source=products.db;Version=3;"))
                {
                    connection.Open();

                    const string sql =
                        "INSERT INTO Product (Name, Price, Category) VALUES (@name, @price, @category)";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@name", P.Name);
                        command.Parameters.AddWithValue("@price", P.Price);
                        command.Parameters.AddWithValue("@category", P.Category);
                        command.ExecuteNonQuery();
                    }

                    // ID neu laden
                    P.Id = (int)connection.LastInsertRowId;
                }

                _products.Add(P);
                StatusText.Text = "Produkt hinzugefügt";
            }
            catch
            {
                StatusText.Text = "Fehler: Preis muss eine Zahl sein";
            }
        }

        private void UpdateProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductList.SelectedItem is Product selected)
            {
                try
                {
                    selected.Name = NameBox.Text;
                    selected.Price = double.Parse(PriceBox.Text);
                    selected.Category = CategoryBox.Text;

                    using (var connection = new SQLiteConnection("Data Source=products.db;Version=3;"))
                    {
                        connection.Open();

                        const string sql =
                            "UPDATE Product SET Name=@name, Price=@price, Category=@category WHERE Id=@id";

                        using (var command = new SQLiteCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@name", selected.Name);
                            command.Parameters.AddWithValue("@price", selected.Price);
                            command.Parameters.AddWithValue("@category", selected.Category);
                            command.Parameters.AddWithValue("@id", selected.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    ProductList.Items.Refresh();
                    StatusText.Text = "Produkt aktualisiert";
                }
                catch
                {
                    StatusText.Text = "Fehler: Preis muss eine Zahl sein";
                }
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductList.SelectedItem is Product selected)
            {
                using (var connection = new SQLiteConnection("Data Source=products.db;Version=3;"))
                {
                    connection.Open();

                    const string sql = "DELETE FROM Product WHERE Id=@id";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", selected.Id);
                        command.ExecuteNonQuery();
                    }
                }

                _products.Remove(selected);
                StatusText.Text = "Produkt gelöscht";
            }
        }

        private void ProductList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductList.SelectedItem is Product selected)
            {
                NameBox.Text = selected.Name;
                PriceBox.Text = selected.Price.ToString();
                CategoryBox.Text = selected.Category;
            }
        }
    }
}
