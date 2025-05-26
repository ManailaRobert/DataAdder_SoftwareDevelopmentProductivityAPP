
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using ClassLibrary_SoftwareDevelopmentProductivityAPP.Models;
using Bogus;
using Bogus.Extensions.Romania;
using System.Text;
using System.Globalization;
using System.Text;
using ClassLibrary_SoftwareDevelopmentProductivityAPP.DataTransferObjects_DTOs;
using Bogus.DataSets;
using static System.Net.Mime.MediaTypeNames;
using SoftwareDevelopmentProductivityAPP.Utils;
using System.ComponentModel.DataAnnotations;

namespace DataAdder_SoftwareDevelopmentProductivityAPP
{


    public class Program
    {
        static DBContext _context;
        static IMapper _mapper;

        public static void Main(string[] args)
        {
            //Config stuff.
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<DBContext>();
            optionsBuilder.UseSqlServer(connectionString);

            _context = new DBContext(optionsBuilder.Options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = mapperConfig.CreateMapper();


            Start(3,
                3,
                3,
                12,
                19,
                2,
                3,
                2,
                2
                );
            Console.ReadKey();  
        }

        static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        private static async void Start(int nrFirmeNoi, 
            int Proiecte_PerFirma = 1, 
            int Functionalitati_PerProiect = 3, 
            int Sarcini_PerAngajat_Min = 12,
            int Sarcini_PerAngajat_Max = 19,
            int EchipeAngajatiNoi =  0,
            int Dezvoltatori_PerEchipa = 3,
            int Designeri_PerEchipa = 2,
            int Testeri_PerEchipa = 2)
        {
           
           


        }

        private static Angajati newAngajat(string Post)
        {
            var faker = new Faker("ro");

            var providers = new List<string>()
            {
                "yahoo.com",
                "gmail.com",
                "outlook.com"
            };

            var serieBuletin = new Dictionary<string, string[]>
            {
                { "B", new[] { "București" } },
                { "CJ", new[] { "Cluj-Napoca", "Dej", "Turda" } },
                { "IS", new[] { "Iași", "Pașcani" } },
                { "TM", new[] { "Timișoara", "Lugoj" } },
                { "BV", new[] { "Brașov", "Făgăraș" } },
                { "GL", new[] { "Galați", "Tecuci" } },
                { "CT", new[] { "Constanța", "Mangalia" } },
                { "AG", new[] { "Pitești", "Curtea de Argeș" } },
                { "NT", new[] { "Piatra Neamț", "Roman" } },
                { "PH", new[] { "Ploiești", "Câmpina" } }
            };

            string nume = faker.Name.FirstName();
            string prenume = faker.Name.LastName();
            string provider = faker.PickRandom(providers);
            string email = faker.Internet.Email(nume, prenume, "yahoo.com");
            string numarDeTelefon = faker.Phone.PhoneNumber();
            string localitate = "Bucuresti";
            string stradaSiNumar = $"Str. {faker.Address.StreetName()}, Nr. {faker.Address.BuildingNumber()}";
            string cnp = faker.Person.Cnp();

            string serieCIBI = "B" + faker.Random.Number(100000, 999999);

            nume = RemoveDiacritics(nume);
            prenume = RemoveDiacritics(prenume);
            email = RemoveDiacritics(email);
            stradaSiNumar = RemoveDiacritics(stradaSiNumar);


            int idPost;
            switch (Post)
            {
                case "Manager":
                    idPost = 1;
                    break;

                case "Designer":
                    idPost = 2;
                    break;

                case "Arhitect":
                    idPost = 3;
                    break;

                case "Dev":
                    idPost = 4;
                    break;

                case "Tester":
                    idPost = 5;
                    break;

                default:
                    throw new Exception("Post invalid");
                    break;
            }

            Angajati Angajat = new Angajati()
            {
                NumeSiPrenume = $"{nume} {prenume}",
                Email = email,
                NumarDeTelefon = numarDeTelefon,
                Localitate = localitate,
                Strada_si_numar = stradaSiNumar,
                IDPost = idPost,
                CNP = cnp,
                SerieCIBI = serieCIBI
            };
            return Angajat;
        }
    
        private static void displayAngajat(Angajati ang)
        {
            string text = $"Nume si Prenume:{ang.NumeSiPrenume}" +
                $"Email: {ang.Email}" +
                $"Numar de telefon: {ang.NumarDeTelefon}" +
                $"Localitate: {ang.Localitate}" +
                $"Strada_si_numar: {ang.Strada_si_numar}" +
                $"IDPost: {ang.IDPost}" +
                $"CNP: {ang.CNP}" +
                $"SerieCiBi: {ang.SerieCIBI}";

            Console.WriteLine(text);    
        }
    
        private static Firme newFirma()
        {
            var faker = new Faker("ro");

            var providers = new List<string>()
            {
                "yahoo.com",
                "gmail.com",
                "outlook.com"
            };

            var ListaOrase = new List<string>
            {
                "București","Cluj-Napoca", "Iași","Brașov", "Galați", 
                "Constanța", "Mangalia","Pitești", "Curtea de Argeș",
                "Ploiești", "Câmpina","Piatra Neamț",
            };

            string denumire = faker.Company.CompanyName();
            string cui = faker.Random.Number(10000000, 99999999).ToString();
            string email = faker.Internet.Email(denumire,"", faker.PickRandom(providers));
            string numarDeTelefon = faker.Phone.PhoneNumber();
            string tara = "Romania";
            string localitate = faker.PickRandom(ListaOrase);
            string stradaSiNumar = $"Str. {faker.Address.StreetName()}, Nr. {faker.Address.BuildingNumber()}";

            Firme firma = new Firme()
            {
                Denumire = denumire,
                CUI = cui,
                Email = email,
                NumarDeTelefon= numarDeTelefon,
                Tara = tara,
                Localitate = localitate,
                Strada_si_numar = stradaSiNumar
               
            };  

            return firma;
        }

        private static void displayFirma(Firme firma)
        {
            string text = $"Denumire:{firma.Denumire}" +
                $"CUI: {firma.CUI}" +
                $"Email: {firma.Email}" +
                $"Numar de telefon: {firma.NumarDeTelefon}" +
                $"Tara: {firma.Tara}" +
                $"Localitate: {firma.Localitate}" +
                $"Strada_si_numar: {firma.Strada_si_numar}";

            Console.WriteLine(text);

        }

        private static Proiecte newProiect(int NrProiect,int CodFirma ,DateTime DataIncepere, DateTime DataFinalizare, DateTime DataDeFinalizat)
        {
            string denumire = $"Proiectul {NrProiect}";
            string descriere = $"Descrierea proiectului {NrProiect}";

            int dataIncepere = DateActions.ConvertToIntFormat(DataIncepere);
            int dataFinalizare = DateActions.ConvertToIntFormat(DataFinalizare);
            int dataDeFinalizat = DateActions.ConvertToIntFormat(DataDeFinalizat);
            int codFirma = CodFirma;

            Proiecte proiect = new Proiecte()
            {
                Denumire = denumire,
                Descriere = descriere,
                DataIncepere= dataIncepere,
                DataFinalizare= dataFinalizare,
                DataDeFinalizat= dataDeFinalizat,
                CODFirma= codFirma
            };

            return proiect;
        }

        private static void displayProiect(Proiecte proiect)
        {
            string text = $"Denumire:{proiect.Denumire}" +
                $"Descriere: {proiect.Descriere}" +
                $"Data Incepere: {DateActions.ConvertToDateTime(proiect.DataIncepere).ToString("dd-MM-yyyy")}" +
                $"Data Finalizare: {DateActions.ConvertToDateTime((int)proiect.DataFinalizare).ToString("dd-MM-yyyy")}" +
                $"Date De Finalizat: {DateActions.ConvertToDateTime(proiect.DataDeFinalizat).ToString("dd-MM-yyyy")}" +
                $"CodFirma: {proiect.CODFirma}";

            Console.WriteLine(text);
        }

        private static Functionalitati newFunctionalitate(int NrFunctionalitate ,int NrProiect)
        {
            string denumire = $"Functionalitatea {NrFunctionalitate}";
            string descriere = $"Descrierea functionalitatii {NrFunctionalitate}";


            Functionalitati func = new Functionalitati()
            {
                Denumire = denumire,
                Descriere = descriere,
                NrProiect = NrProiect
            };

            return func;

        }

        private static void displayFunctionalitate(Functionalitati func)
        {
            string text = $"Denumire:{func.Denumire}" +
                $"Descriere: {func.Descriere}" +
                $"NrProiect: {func.NrProiect}";

            Console.WriteLine(text);
        }


        private static Sarcini newSarcina(int NrSarcina,int IdFunctionalitate, int MarcaAngajat, DateTime DataCreare, string GradDificultate, string GradUrgenta)
        {
            var faker = new Faker("ro");


            var ListaGradeUrgenta = new Dictionary<string, int>()
            {
                {"Mica",1},
                {"Medie",2 },
                {"Maxima",3 }
            };

            var ListaGradeDificultate = new Dictionary<string, int>()
            {
                {"Usoara",1},
                {"Medie",2 },
                {"Grea",3 }
            };

            List<List<Dictionary<string, int>>> MatriceDateDeFinalizare = new List<List<Dictionary<string, int>>>
            {
                new List<Dictionary<string, int>> // Dificultate Usoara
                {
                    new Dictionary<string, int> { { "min",2 }, { "max",7} },// Urgenta Mica
                    new Dictionary<string, int> { { "min",2 }, { "max",6} },// Urgenta Medie
                    new Dictionary<string, int> { { "min",2 }, { "max",5} } // Urgenta Maxima
                },

                new List<Dictionary<string, int>> // Dificultate Medie
                {
                    new Dictionary<string, int> { { "min",3 }, { "max",7} },// Urgenta Mica
                    new Dictionary<string, int> { { "min",3 }, { "max",6} },// Urgenta Medie
                    new Dictionary<string, int> { { "min",3 }, { "max",5} } // Urgenta Maxima
                },

                new List<Dictionary<string, int>>// Dificultate Grea
                {
                    new Dictionary<string, int> { { "min",4 }, { "max",7} },// Urgenta Mica
                    new Dictionary<string, int> { { "min",4 }, { "max",6} },// Urgenta Medie
                    new Dictionary<string, int> { { "min",4 }, { "max",5} } // Urgenta Maxima
                }
            };

            string denumire = $"Sarcina {NrSarcina}";
            string descriere = $"Descrierea sarcinii {NrSarcina}";


            int marcaAngajat = MarcaAngajat;
            decimal calificativ = faker.Random.Decimal((decimal)0.5, (decimal)1.0);
            int idGradUrgenta;
            int idGradDificultate;
            try
            {
                idGradUrgenta = ListaGradeUrgenta[$"{GradUrgenta}"];
            }
            catch (Exception ex)
            {
                throw new Exception("Nu exista gradul de urgenta");
            }
            try
            {
                idGradDificultate = ListaGradeDificultate[$"{GradDificultate}"];
            }
            catch (Exception ex)
            {
                throw new Exception("Nu exista gradul de dificultate");
            }

            int dataCreare = DateActions.ConvertToIntFormat(DataCreare);

            // Se ia min si max de zile adaugate pentru rezolvare
            int minZileLucrate = MatriceDateDeFinalizare[idGradDificultate][idGradUrgenta]["min"];
            int maxZileLucrate = MatriceDateDeFinalizare[idGradDificultate][idGradUrgenta]["max"];

            // Se randomizeaza un nr intre acel min si max
            int zileAdaugate_dataDeFinalizat = faker.Random.Number(minZileLucrate, maxZileLucrate);
            
            //Se adauga nr random la data de creare.
            DateTime DataDeFinalizat = DataCreare.AddDays(zileAdaugate_dataDeFinalizat);
            int dataDeFinalizat = DateActions.ConvertToIntFormat(DataDeFinalizat);

            // Se randomizeaza un nr de zile ce se va scade sau adauga din data de finalizat 
            int zile_dataFinalizare = faker.Random.Number(-3, 3);

            //Se adauga / scad zilele randomizate
            DateTime DataFinalizare = DataDeFinalizat.AddDays(-zile_dataFinalizare);
            int dataFinalizare = DateActions.ConvertToIntFormat(DataFinalizare);

            Sarcini sarcina = new Sarcini()
            {
                Denumire = denumire,
                Descriere = descriere,
                DataCreare = dataCreare,
                DataFinalizare = dataFinalizare,
                DataDeFinalizat = dataDeFinalizat,
                IDFunctionalitate = IdFunctionalitate,
                IDGradDificultate = idGradDificultate,
                IDGradUrgentaSarcina = idGradUrgenta,
            };

            return sarcina;
        }

        private static List<PerioadeDeLucru> newPerioadeDeLucru(Sarcini sarcina)
        {
            var faker = new Faker("ro");

            List<PerioadeDeLucru> listaPerioade = new List<PerioadeDeLucru>() ;
            DateTime dataCurentaInregistrare = DateActions.ConvertToDateTime(sarcina.DataCreare);
            DateTime dataFinalizarePerioada = DateActions.ConvertToDateTime((int)sarcina.DataFinalizare) ;
            while (dataCurentaInregistrare <= dataFinalizarePerioada)
            {
                int oraIncepere = faker.Random.Number(8, 10);
                int oraTerminare = faker.Random.Number(14, 16);
                int oreLucrate = oraTerminare - oraIncepere;
                listaPerioade.Add(new PerioadeDeLucru()
                {
                    Data = DateActions.ConvertToIntFormat(dataCurentaInregistrare),
                    OraIncepere = oraIncepere,
                    OraTerminare = oraTerminare,
                    OreLucrate = oreLucrate,
                    MarcaAngajat = (int)sarcina.MarcaAngajat,
                    IDSarcina = sarcina.IDSarcina
                });
                dataCurentaInregistrare.AddDays(1);
            }

            return listaPerioade;
        }

    }
}
