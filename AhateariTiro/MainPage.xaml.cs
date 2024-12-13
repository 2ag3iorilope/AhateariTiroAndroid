namespace AhateariTiro
{
    public partial class MainPage : ContentPage
    {
        private Random random = new Random(); // Patuek sortzeko erabilitako Random objektua
        private List<Duck> aktiboPatuak = new List<Duck>(); // Jokoan aktibo dauden patuen zerrenda
        private IDispatcherTimer jokoTimerra; // Jokoaren denbora-timerra
        private IDispatcherTimer kontagailuTimerra; // Kontagailuaren timerra
        private bool jokoaHasita = false; // Jokoaren hasiera kontrolatzeko aldagaia
        private int puntuazioa = 0; // Puntua
        private int kontagailuDenbora = 60; // Kontagailuaren iraupena segundotan

        public MainPage()
        {
            InitializeComponent(); // Osagaiak hasieratu
            InitializeGame(); // Jokoaren inicializazioa

            // "Hasi" botoiaren klik ekintza asignatzea
            var hasieratuBotoia = this.FindByName<Button>("btnHasi");
            hasieratuBotoia.Clicked += (s, e) => JokoHasiera();

            // "Amaitu" botoiaren klik ekintza asignatzea
            var amaituBotoia = this.FindByName<Button>("btnAmaitu");
            amaituBotoia.Clicked += (s, e) => JokoaAmaitu();
        }

        private void InitializeGame()
        {
            jokoTimerra = Application.Current.Dispatcher.CreateTimer(); // Jokoaren timerra sortu
            jokoTimerra.Interval = TimeSpan.FromMilliseconds(50); // Timerra 50 milisegundoko iraupena
            jokoTimerra.Tick += JokoBueltatu; // Jokoaren loop-a esleitu
            jokoTimerra.Start(); // Timerra abiarazi

            kontagailuTimerra = Application.Current.Dispatcher.CreateTimer(); // Kontagailuaren timerra sortu
            kontagailuTimerra.Interval = TimeSpan.FromSeconds(1); // Timerra 1 segundoko iraupena
            kontagailuTimerra.Tick += Kontagailua; // Kontagailuaren tick-a esleitu
        }

        private void JokoHasiera()
        {
            if (!jokoaHasita) // Jokoak ez badu hasi
            {
                jokoaHasita = true; // Jokoaren hasiera aldatu
                puntuazioa = 0; // Puntua berresetu
                kontagailuDenbora = 60; // Kontagailua berresetu
                lblDenabora.Text = kontagailuDenbora.ToString(); // Hasierako denbora erakutsi
                lblPuntuak.Text = "Puntuak: 0"; // Hasierako puntuazioa erakutsi
                AhateaSortu(); // Patuen sorrera hasi
                kontagailuTimerra.Start(); // Kontagailua hasi
            }
        }

        private async void AhateaSortu()
        {
            while (jokoaHasita) // Jokoa hasi bada
            {
                await Task.Delay(random.Next(2000, 5000)); // Patua sortzeko atzerapena
                PatuaSortu(); // Patua sortu
            }
        }

        private void Kontagailua(object sender, EventArgs e)
        {
            kontagailuDenbora--; // Kontagailua murriztu
            lblDenabora.Text = kontagailuDenbora.ToString(); // Denbora eguneratu interfazearen gainean

            if (kontagailuDenbora <= 0) // Denbora amaitu bada
            {
                JokoaAmaitu(); // Jokoa amaitu
            }
        }

        [Obsolete]
        private void JokoaAmaitu()
        {
            jokoaHasita = false; // Jokoa amaitu
            kontagailuTimerra.Stop(); // Kontagailua gelditu

            // Puntuazioa erakutsi alerta batean
            DisplayAlert("Partida Amaituta", $"zure puntuazioa: {puntuazioa}", "Berriro jolastu", "Itxi").ContinueWith(t =>
            {
                if (t.Result) // "Jugar de Nuevo" hautatu bada
                {
                    JokoaBerresetu(); // Jokoa berresetu
                }
                else
                {
                    // Aplikazioa itxi
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Application.Current.Quit(); // Aplikazioa ixteko
                    });
                }
            });
        }

        private void JokoaBerresetu()
        {
            aktiboPatuak.Clear(); // Aktibo dauden patuak garbitu
            GameArea.Children.Clear(); // Jokoaren eremua garbitu
            lblDenabora.Text = "60"; // Denbora etiketan berresetu
            lblPuntuak.Text = "Puntuak: 0"; // Puntuazioa berresetu
            puntuazioa = 0; // Puntua berresetu
        }

        private void PatuaSortu()
        {
            var patua = new Duck // Patua sortu
            {
                IsSpeedyDuck = random.Next(2) == 0, // Patua azkarrak edo normala den zehaztu
                Position = new Point(-50, random.Next(50, (int)GameArea.Height - 50)) // Patua hasierako posizioan
            };

            var patuIrudia = new Image
            {
                Source = patua.IsSpeedyDuck ? "patorapido.png" : "pato.png", // Irudia aukeratu
                WidthRequest = 100,
                HeightRequest = 100
            };

            var tapGestureRecognizer = new TapGestureRecognizer(); // Tap gestua
            tapGestureRecognizer.Tapped += (s, e) =>
            {
                PatuEzabatu(patua); // Patua ezabatu
                puntuazioa++; // Puntua handitu
                lblPuntuak.Text = $"Puntuak: {puntuazioa}"; // Interfazearen puntuazioa eguneratu
            };
            patuIrudia.GestureRecognizers.Add(tapGestureRecognizer); // Gestua irudira gehitu

            patua.Visual = patuIrudia; // Irudia patuari esleitu
            aktiboPatuak.Add(patua); // Patua aktiboen zerrendan gehitu
            GameArea.Children.Add(patuIrudia); // Irudia jokoaren eremuan gehitu

            AbsoluteLayout.SetLayoutBounds(patuIrudia,
                new Rect(patua.Position.X, patua.Position.Y, 50, 50)); // Patuaren posizioa ezarri
        }

        private void JokoBueltatu(object sender, EventArgs e)
        {
            for (int i = aktiboPatuak.Count - 1; i >= 0; i--) // Aktibo dauden patuak erakutsi
            {
                var patua = aktiboPatuak[i]; // Patu aktiboa hartu
                patua.Position = new Point(
                    patua.Position.X + (patua.IsSpeedyDuck ? 8 : 4), // Patuaren posizioa eguneratu
                    patua.Position.Y);

                AbsoluteLayout.SetLayoutBounds(patua.Visual,
                    new Rect(patua.Position.X, patua.Position.Y, 50, 50)); // Irudiaren posizioa ezarri

                // Irudiak pantailatik ateratzen direnean ezabatu
                if (patua.Position.X > GameArea.Width)
                {
                    GameArea.Children.Remove(patua.Visual); // Irudia ezabatu
                    aktiboPatuak.RemoveAt(i); // Patua zerrendetik ezabatu
                }
            }
        }

        private void PatuEzabatu(Duck patua)
        {
            GameArea.Children.Remove(patua.Visual); // Irudia ezabatu
            aktiboPatuak.Remove(patua); // Patu aktiboa ezabatu
        }
    }

    public class Duck
    {
        public bool IsSpeedyDuck { get; set; } // Patu azkarra den ala ez
        public Point Position { get; set; } // Patuaren posizioa
        public Image Visual { get; set; } // Patuaren irudia
    }
}
