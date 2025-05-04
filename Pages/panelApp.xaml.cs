using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Project_Lightning.Windows;

namespace Project_Lightning.Pages
{
    /// <summary>
    /// Lógica de interação para panelUbisoft.xaml
    /// </summary>
    public partial class panelApp : Page
    {

        MainWindow ventanaPrincipal;
        string nombreApp;
        public panelApp(String nomApp, MainWindow mainWindow)
        {
            InitializeComponent();
            
            //ALTERA O NOME DO TEXTO
            txtApp.Text = nomApp;
            //ALTERA A COR DO TEXTO
            switch (nomApp)
            {
                case "UBISOFT": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A0DAD ")); break;
                case "EA": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f74545")); break;
                case "ROCKSTAR": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f7a600")); break;
                case "DENUVO": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#607D8B")); break;
                case "PlayStation": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0300b3")); break;
                case "OTHERS": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8BBD0")); break;

            }

            //INICIALIZA A JANELA PARA TER UMA REFERÊNCIA DA JANELA PRINCIPAL E A VARIÁVEL DE TEXTO
            ventanaPrincipal = mainWindow;
            nombreApp = nomApp;

            ponerJuegos(nomApp);

        }

        //CLASSE JOGO QUE CONTÉM AS INFORMAÇÕES DE CADA JOGO
        public class Juego
        {
            public string name { get; set; }
            public bool launch_steam { get; set; }
            public bool launch_exe { get; set; }
            public string comentarios { get; set; }
            public List<string> programas_necesarios { get; set; }
            public List<string> errores { get; set; }
            public string nombre_fix { get; set; }
            public Dictionary<string, string> custom_images { get; set; }
        }
        

        private async void ponerJuegos(string nomApp)
        {
            var juegosApp = await sacarJuegosDeApp(nomApp);

            colocarBotones(juegosApp);

            //baixarJogo(juegosApp.First());

        }


        //ESTE MÉTODO BUSCA OBTER TODOS OS JOGOS DE UMA ÚNICA COMPANHIA DADA PELO nomApp
        private async Task<Dictionary<string, Juego>> sacarJuegosDeApp(string nomApp)
        {
            string rutaJson = System.IO.Path.GetFullPath(@"..\\..\\data.json");
            string rutaJsonApp = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json");
            string urlJson = "https://raw.githubusercontent.com/LightnigFast/Project-Lightning/main/data.json";

            if (await EsArchivoGitHubDiferente(urlJson, rutaJson))
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string contenidoGitHub = await client.GetStringAsync(urlJson);
                        File.WriteAllText(rutaJson, contenidoGitHub); //ATUALIZA ARQUIVO LOCAL
                        File.WriteAllText(rutaJsonApp, contenidoGitHub); //ATUALIZA ARQUIVO LOCAL
                        //MessageBox.Show("O arquivo data.json foi atualizado do GitHub");
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Erro ao atualizar data.json: " + ex.Message);
                    var ventanaError = new Windows.ErrorDialog("Erro ao atualizar a lista de jogos, tente novamente mais tarde: " + ex.Message, Brushes.Red);
                    ventanaError.ShowDialog();
                }
            }

            //CARREGAR LOCALMENTE
            string json = File.ReadAllText(rutaJsonApp);
            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Juego>>>(json);

            if (data.ContainsKey(nomApp))
            {
                return data[nomApp];
            }

            return new Dictionary<string, Juego>();
        }

        private async Task<bool> EsArchivoGitHubDiferente(string url, string rutaLocal)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string contenidoGitHub = await client.GetStringAsync(url);

                    if (!File.Exists(rutaLocal))
                    {
                        return true; //NÃO EXISTE LOCALMENTE, É DIFERENTE
                    }

                    string contenidoLocal = File.ReadAllText(rutaLocal);
                    return !contenidoLocal.Equals(contenidoGitHub);
                }
            }
            catch (Exception ex)
            {
                var ventanaError = new ErrorDialog("Erro ao comparar arquivos: " + ex.Message, Brushes.Red);
                ventanaError.Show();
                //MessageBox.Show("Erro ao comparar arquivos: " + ex.Message);
                return false;
            }
        }


        //ESTE MÉTODO BUSCA CRIAR TODOS OS BOTÕES, COLOCAR SUA IMAGEM E SEU RESPECTIVO MÉTODO DE CLIQUE
        private void colocarBotones(Dictionary<string, Juego> juegosApp)
        {
            if (juegosApp.Count != 0)
            {
                //LAÇO PARA OBTER TODOS OS JOGOS
                foreach (var juego in juegosApp)
                {
                    //CRIA OS BOTÕES DE CADA JOGO E DEFINE O TAMANHO PADRÃO
                    Button botonJuego = new Button
                    {
                        Width = 198,
                        Height = 298,
                        Margin = new Thickness(17),
                    };

                    //APLICA O ESTILO DEFINIDO NO XAML
                    botonJuego.Style = (Style)FindResource("Boton_juego");

                    //CRIA A IMAGEM QUE IRÁ EM CADA BOTÃO
                    Image imagenJuego = new Image
                    {
                        Width = 198,
                        Height = 298,
                        Stretch = Stretch.Fill
                    };

                    string imagenPersonalizada = null;

                    if (juego.Value != null &&
                        juego.Value.custom_images.TryGetValue("hero_image", out imagenPersonalizada) &&
                        !string.IsNullOrWhiteSpace(imagenPersonalizada))
                    {
                        //SE HOUVER IMAGEM PERSONALIZADA, USA ELA
                        imagenJuego.Source = new BitmapImage(new Uri(imagenPersonalizada));
                    }
                    else
                    {
                        //TENTA CARREGAR A IMAGEM ORIGINAL DO STEAM
                        imagenJuego.Source = new BitmapImage(new Uri("https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/" + juego.Key + "/library_600x900.jpg"));

                        //SE FALHAR O CARREGAMENTO, COLOCA UMA IMAGEM DE BACKUP
                        imagenJuego.ImageFailed += (sender, e) =>
                        {
                            imagenJuego.Stretch = Stretch.Uniform;
                            imagenJuego.Source = new BitmapImage(new Uri("https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/" + juego.Key + "/capsule_184x69.jpg?t=1739176298"));
                        };
                    }

                    //ADICIONA A IMAGEM AO BOTÃO
                    botonJuego.Content = imagenJuego;

                    //EVENTO QUANDO UM BOTÃO É CLICADO
                    botonJuego.Click += (sender, e) =>
                    {

                        ventanaPrincipal.framePrincipal.Navigate(new panelJuego(nombreApp, juego, this, ventanaPrincipal));
                    };

                    //POR FIM, ADICIONA AO PAINEL DE JOGOS
                    panelJuegos.Children.Add(botonJuego);
                }
            }
            else
            {

                TextBlock textBlock = new TextBlock
                {
                    TextAlignment = TextAlignment.Center,
                    FontSize = 40,
                    FontFamily = (FontFamily)this.Resources["FuenteJohnInclinada"],
                    Padding = new Thickness(20)
                };

                //TEXTO NORMAL
                textBlock.Inlines.Add(new Run
                {
                    Text = "NÃO HÁ JOGOS ",
                    Foreground = Brushes.White
                });

                //TEXTO PARA O FOR NOW
                textBlock.Inlines.Add(new Run
                {
                    Text = "POR ENQUANTO",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A0DAD"))

                });

                //TEXTO PARA O EMOJI
                textBlock.Inlines.Add(new Run
                {
                    Text = "🚧",
                    Foreground = Brushes.Yellow
                });

                //CRIA O BINDING DA LARGURA
                Binding binding = new Binding("ActualWidth")
                {
                    Source = panelJuegos
                };
                textBlock.SetBinding(FrameworkElement.WidthProperty, binding);

                //CRIA A BORDA
                Border border = new Border
                {
                    Background = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Child = textBlock
                };

                panelJuegos.Children.Add(border);

            }
            
        }



    }
}
