
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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

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


            Dictionary<string,Dictionary<string, int>> nrSarciniPerAngajatPerPost = new Dictionary<string, Dictionary<string, int>>
            {
                ["Design"] = new Dictionary<string, int>
                {
                    ["min"] = 1,
                    ["max"] = 2, 
                },
                ["Dev"] = new Dictionary<string, int>
                {
                    ["min"] = 2,
                    ["max"] = 3,
                },
                ["Tester"] = new Dictionary<string, int>
                {
                    ["min"] = 2,
                    ["max"] = 3,
                },
            };


            Start2(20220101,
                20221220,
                20221228,
                12,
                19,
                3,
                2,
                2
                );
            Console.WriteLine("Finished");
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
        private static async Task Start(int dataIncepere_Proiect, int dataFinalizare_Proiect, int dataDeTerminat_Proiect,
            int Functionalitati_PerProiect = 3, 
            int Sarcini_PerAngajat_Min = 12,
            int Sarcini_PerAngajat_Max = 19,
            int Dezvoltatori_PerEchipa = 3,
            int Designeri_PerEchipa = 2,
            int Testeri_PerEchipa = 2)
        {
            var faker = new Faker("ro");

            int nrFirmeNoi = 1;
            int Proiecte_PerFirma = 1;

            for (int f = 1; f <= nrFirmeNoi; f++)
            {
                //Creare f firme noi
                Firme firma = newFirma();
                await _context.Firme.AddAsync(firma);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Eroare la salvare firmei \n{ex.Message}");
                    Environment.Exit(0);
                }

                for (int p = 1; p <= Proiecte_PerFirma;p++)
                {

                    //creare Proiecte
                    int nrProiect = _context.Proiecte.Count()+1;
                    DateTime dataIncepereProiect = DateActions.ConvertToDateTime(dataIncepere_Proiect);
                    DateTime dataFinalizareProiect = DateActions.ConvertToDateTime(dataFinalizare_Proiect);
                    DateTime dataDeFinalizatProiect = DateActions.ConvertToDateTime(dataDeTerminat_Proiect);
                    Proiecte proiectNou = newProiect(nrProiect, firma.CODFirma, dataIncepereProiect, dataFinalizareProiect, dataDeFinalizatProiect);
                    
                    await _context.Proiecte.AddAsync(proiectNou);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Eroare la salvarea proiectului \n{ex.Message}");
                        Environment.Exit(0);
                    }
                    int idProiect = proiectNou.NrProiect;

                    // Angajati inactivi
                    List<Angajati> angajati = await _context.Angajati
                        .Include(a => a.Membri_Dezvoltare)
                        .Where(a => !a.Membri_Dezvoltare.Any(m => m.Status == "activ") || a.Membri_Dezvoltare.Count == 0)
                        .ToListAsync();

                    //Angajati inactivi pe roluri
                    List<Angajati> manageri = await _context.Angajati
                        .Where(a => a.IDPost == 1)
                        .ToListAsync();

                    List<Angajati> developeri = angajati.Where(a => a.IDPost == 4).ToList();
                    List<Angajati> designeri = angajati.Where(a => a.IDPost == 2).ToList();
                    List<Angajati> testeri = angajati.Where(a => a.IDPost == 5).ToList();
                    List<Angajati> arhitecti = angajati.Where(a => a.IDPost == 3).ToList();

                    //Creare angajati noi
                    while (developeri.Count < Dezvoltatori_PerEchipa)
                    {
                        Angajati angajatNou = newAngajat("Dev");
                        await _context.Angajati.AddAsync(angajatNou);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eroare la salvare angajat nou Dev \n{ex.Message}");
                            Environment.Exit(0);
                        }
                        developeri.Add(angajatNou);
                    }
                    while (designeri.Count < Designeri_PerEchipa)
                    {
                        Angajati angajatNou = newAngajat("Designer");
                        await _context.Angajati.AddAsync(angajatNou);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eroare la salvare angajat nou Designer \n{ex.Message}");
                            Environment.Exit(0);
                        }
                        designeri.Add(angajatNou);

                    }
                    while (testeri.Count < Testeri_PerEchipa)
                    {
                        Angajati angajatNou = newAngajat("Tester");
                        await _context.Angajati.AddAsync(angajatNou);
                        try
                        {
                            await _context.SaveChangesAsync();

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eroare la salvare angajat nou Tester \n{ex.Message}");
                            Environment.Exit(0);
                        }
                        testeri.Add(angajatNou);

                    }
                    while (arhitecti.Count < 1)
                    {
                        Console.WriteLine("Infinit Arhitecti");
                        Angajati angajatNou = newAngajat("Arhitect");

                        await _context.Angajati.AddAsync(angajatNou);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eroare la salvare angajat nou Arhitect \n{ex.Message}");
                            Environment.Exit(0);
                        }
                        arhitecti.Add(angajatNou);
                    }
                    
                    List<Angajati> membriDezvoltare = new List<Angajati>();

                    for(int i = 0; i < Dezvoltatori_PerEchipa; i++)
                    {
                        membriDezvoltare.Add(developeri[i]);
                    }

                    for (int i = 0; i < Designeri_PerEchipa; i++)
                    {
                        membriDezvoltare.Add(designeri[i]);
                    }

                    for (int i = 0; i < Testeri_PerEchipa; i++)
                    {
                        membriDezvoltare.Add(testeri[i]);
                    }

                    //Adaugare membri in echipa
                    await adaugaMembriInEchipa(proiectNou,membriDezvoltare);
                    await adaugaMembriInEchipa(proiectNou, new List<Angajati>() 
                    { faker.PickRandom(manageri), faker.PickRandom(arhitecti) });

                    //creare functionalitatilor
                    for (int i = 0; i < Functionalitati_PerProiect; i++)
                    {
                        Functionalitati functNoua = newFunctionalitate(i + 1, proiectNou.NrProiect);

                        await _context.Functionalitati.AddAsync(functNoua);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Eroare la salvare functionalitati \n{ex.Message}");
                        Environment.Exit(0);
                    }

                    List<int> listaIdFunct = await _context.Functionalitati
                        .Where(f => f.NrProiect == proiectNou.NrProiect)
                        .Select(f => f.IDFunctionalitate)
                        .ToListAsync();


                    // Creare sarcini pentru angajati

                    Console.WriteLine($"Firma {firma.CODFirma} \n" +
                        $"Proiect {nrProiect} \n ---------------\n ");

                    List<int> ListaMarcaAngajati = membriDezvoltare.Select(a => a.MarcaAngajat).ToList();
                    membriDezvoltare.Clear();

                    foreach (int MarcaAng in ListaMarcaAngajati)
                    {
                        //Creare sarcini
                        DateTime dataCreareSarcina = dataIncepereProiect;
                        int counterSarcini = 1;
                        List<Sarcini> listaSarcini = new List<Sarcini>();
                        while (dataCreareSarcina < dataFinalizareProiect)
                        {
                            int IdFunct = faker.PickRandom(listaIdFunct);

                            Sarcini sarcinaNoua = newSarcina(counterSarcini, IdFunct, MarcaAng, dataCreareSarcina);

                            if (sarcinaNoua.DataFinalizare > DateActions.ConvertToIntFormat(dataFinalizareProiect)
                                || sarcinaNoua.DataDeFinalizat > DateActions.ConvertToIntFormat(dataFinalizareProiect))
                                break;

                            //move no next.
                            dataCreareSarcina = DateActions.ConvertToDateTime((int)sarcinaNoua.DataFinalizare).AddDays(1);
                            counterSarcini++;

                            listaSarcini.Add(sarcinaNoua);

                            List<PerioadeDeLucru> listaPerioadeLucru = new List<PerioadeDeLucru>();
                            // Salvare sarcini peste 50
                            if (listaSarcini.Count >= 50) 
                            {
                                _context.Sarcini.AddRange(listaSarcini);
                                try
                                {
                                    await _context.SaveChangesAsync();
                                    _context.ChangeTracker.Clear();
                                    Console.WriteLine("Salvare Sarcini peste 50.");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Eroare la salvare sarcinilor\n{ex.Message}");
                                    Environment.Exit(0);
                                }

                                //Creare Perioade de lucru -- salvare peste 50
                                foreach (Sarcini s in listaSarcini)
                                {
                                    List<PerioadeDeLucru> temp = newPerioadeDeLucru(s);
                                    listaPerioadeLucru.AddRange(temp);
                                    temp.Clear();
                                    
                                    if (listaPerioadeLucru.Count >= 50)
                                    {
                                        _context.PerioadeDeLucru.AddRange(listaPerioadeLucru);
                                        try
                                        {
                                            await _context.SaveChangesAsync();
                                            _context.ChangeTracker.Clear();
                                            Console.WriteLine("Salvare Sarcini peste 50. / Perioade de lucru peste 50");
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Eroare la salvare perioadelor de lucru \n{ex.Message}");
                                            Environment.Exit(0);
                                        }
                                        listaPerioadeLucru.Clear();
                                    }

                                }
                                
                                // Salvare perioade  sub 50
                                if (listaPerioadeLucru.Count > 0)
                                {
                                    _context.PerioadeDeLucru.AddRange(listaPerioadeLucru);
                                    try
                                    {
                                        await _context.SaveChangesAsync();
                                        _context.ChangeTracker.Clear();
                                        Console.WriteLine("Salvare Sarcini peste 50. / Perioade de lucru ramase 50");

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Eroare la salvare perioadelor de lucru \n{ex.Message}");
                                        Environment.Exit(0);
                                    }
                                    listaPerioadeLucru.Clear();
                                }

                                listaSarcini.Clear();
                            }
                        }

                        //Salvare Sarcini sub 50 ---- Creare perioade lucru
                        if (listaSarcini.Count > 0)
                        {
                            List<PerioadeDeLucru> listaPerioadeLucru  = new List<PerioadeDeLucru>();

                            _context.Sarcini.AddRange(listaSarcini);
                            _context.SaveChanges();
                            _context.ChangeTracker.Clear();

                            foreach (Sarcini s in listaSarcini)
                            {
                                List<PerioadeDeLucru> temp = newPerioadeDeLucru(s);
                                listaPerioadeLucru.AddRange(temp);
                                temp.Clear();
                                //Salvare Perioade peste 50
                                if (listaPerioadeLucru.Count >= 50)
                                {
                                    _context.PerioadeDeLucru.AddRange(listaPerioadeLucru);
                                    try
                                    {
                                        await _context.SaveChangesAsync();
                                        _context.ChangeTracker.Clear();

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Eroare la salvare perioadelor de lucru \n{ex.Message}");
                                        Environment.Exit(0);
                                    }
                                    listaPerioadeLucru.Clear();
                                }
                            }
                            //Salvare perioade sub 50
                            if (listaPerioadeLucru.Count > 0)
                            {
                                _context.PerioadeDeLucru.AddRange(listaPerioadeLucru);
                                try
                                {
                                    await _context.SaveChangesAsync();
                                    _context.ChangeTracker.Clear();

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Eroare la salvare perioadelor de lucru \n{ex.Message}");
                                    Environment.Exit(0);
                                }
                                listaPerioadeLucru.Clear();
                            }
                        }

                        _context.SaveChanges();
                        _context.ChangeTracker.Clear();

                    }
                }
            }

            await _context.SaveChangesAsync();  
            Console.WriteLine("Terminat Inserare");
        }

        //NeOptimizat
        private static async Task Start2(int dataIncepere_Proiect, int dataFinalizare_Proiect, int dataDeTerminat_Proiect,
            int Functionalitati_PerProiect = 3,
            int Sarcini_PerAngajat_Min = 12,
            int Sarcini_PerAngajat_Max = 19,
            int Dezvoltatori_PerEchipa = 3,
            int Designeri_PerEchipa = 2,
            int Testeri_PerEchipa = 2)
        {
            var faker = new Faker("ro");

            int nrFirmeNoi = 1;
            int Proiecte_PerFirma = 1;

            for (int f = 1; f <= nrFirmeNoi; f++)
            {
                //Creare f firme noi
                Firme firma = newFirma();
                await _context.Firme.AddAsync(firma);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Eroare la salvare firmei \n{ex.Message}");
                    Environment.Exit(0);
                }

                for (int p = 1; p <= Proiecte_PerFirma; p++)
                {

                    //creare Proiecte
                    int nrProiect = _context.Proiecte.Count() + 1;
                    DateTime dataIncepereProiect = DateActions.ConvertToDateTime(dataIncepere_Proiect);
                    DateTime dataFinalizareProiect = DateActions.ConvertToDateTime(dataFinalizare_Proiect);
                    DateTime dataDeFinalizatProiect = DateActions.ConvertToDateTime(dataDeTerminat_Proiect);
                    Proiecte proiectNou = newProiect(nrProiect, firma.CODFirma, dataIncepereProiect, dataFinalizareProiect, dataDeFinalizatProiect);

                    await _context.Proiecte.AddAsync(proiectNou);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Eroare la salvarea proiectului \n{ex.Message}");
                        Environment.Exit(0);
                    }
                    int idProiect = proiectNou.NrProiect;

                    // Angajati inactivi
                    List<Angajati> angajati = await _context.Angajati
                        .Include(a => a.Membri_Dezvoltare)
                        .Where(a => !a.Membri_Dezvoltare.Any(m => m.Status == "activ") || a.Membri_Dezvoltare.Count == 0)
                        .ToListAsync();

                    //Angajati inactivi pe roluri
                    List<Angajati> manageri = await _context.Angajati
                        .Where(a => a.IDPost == 1)
                        .ToListAsync();

                    List<Angajati> developeri = angajati.Where(a => a.IDPost == 4).ToList();
                    List<Angajati> designeri = angajati.Where(a => a.IDPost == 2).ToList();
                    List<Angajati> testeri = angajati.Where(a => a.IDPost == 5).ToList();
                    List<Angajati> arhitecti = angajati.Where(a => a.IDPost == 3).ToList();

                    //Creare angajati noi
                    while (developeri.Count < Dezvoltatori_PerEchipa)
                    {
                        Angajati angajatNou = newAngajat("Dev");
                        await _context.Angajati.AddAsync(angajatNou);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eroare la salvare angajat nou Dev \n{ex.Message}");
                            Environment.Exit(0);
                        }
                        developeri.Add(angajatNou);
                    }
                    while (designeri.Count < Designeri_PerEchipa)
                    {
                        Angajati angajatNou = newAngajat("Designer");
                        await _context.Angajati.AddAsync(angajatNou);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eroare la salvare angajat nou Designer \n{ex.Message}");
                            Environment.Exit(0);
                        }
                        designeri.Add(angajatNou);

                    }
                    while (testeri.Count < Testeri_PerEchipa)
                    {
                        Angajati angajatNou = newAngajat("Tester");
                        await _context.Angajati.AddAsync(angajatNou);
                        try
                        {
                            await _context.SaveChangesAsync();

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eroare la salvare angajat nou Tester \n{ex.Message}");
                            Environment.Exit(0);
                        }
                        testeri.Add(angajatNou);

                    }
                    while (arhitecti.Count < 1)
                    {
                        Console.WriteLine("Infinit Arhitecti");
                        Angajati angajatNou = newAngajat("Arhitect");

                        await _context.Angajati.AddAsync(angajatNou);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eroare la salvare angajat nou Arhitect \n{ex.Message}");
                            Environment.Exit(0);
                        }
                        arhitecti.Add(angajatNou);
                    }
                    
                    // Adaugare angajati in echipa
                    List<Angajati> membriDezvoltare = new List<Angajati>();
                    for (int i = 0; i < Dezvoltatori_PerEchipa; i++)
                    {
                        membriDezvoltare.Add(developeri[i]);
                    }
                    for (int i = 0; i < Designeri_PerEchipa; i++)
                    {
                        membriDezvoltare.Add(designeri[i]);
                    }
                    for (int i = 0; i < Testeri_PerEchipa; i++)
                    {
                        membriDezvoltare.Add(testeri[i]);
                    }

                    await adaugaMembriInEchipa(proiectNou, membriDezvoltare);
                    await adaugaMembriInEchipa(proiectNou, new List<Angajati>()
                    { faker.PickRandom(manageri), faker.PickRandom(arhitecti) });


                    //creare functionalitatilor
                    for (int i = 0; i < Functionalitati_PerProiect; i++)
                    {
                        Functionalitati functNoua = newFunctionalitate(i + 1, proiectNou.NrProiect);

                        await _context.Functionalitati.AddAsync(functNoua);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Eroare la salvare functionalitati \n{ex.Message}");
                        Environment.Exit(0);
                    }

                    List<int> listaIdFunct = await _context.Functionalitati
                        .Where(f => f.NrProiect == proiectNou.NrProiect)
                        .Select(f => f.IDFunctionalitate)
                        .ToListAsync();


                    // Creare sarcini pentru angajati

                    Console.WriteLine($"Firma {firma.CODFirma} \n" +
                        $"Proiect {nrProiect} \n ---------------\n ");

                    List<int> ListaMarcaAngajati = membriDezvoltare.Select(a => a.MarcaAngajat).ToList();
                    membriDezvoltare.Clear();

                    foreach (int MarcaAng in ListaMarcaAngajati)
                    {
                        //Creare sarcini pentru fiecare angajat
                        DateTime dataCreareSarcina = dataIncepereProiect;
                        int counterSarcini = 1;
                        List<Sarcini> listaSarcini = new List<Sarcini>();

                        while (dataCreareSarcina < dataFinalizareProiect)
                        {
                            int IdFunct = faker.PickRandom(listaIdFunct);

                            Sarcini sarcinaNoua = newSarcina(counterSarcini, IdFunct, MarcaAng, dataCreareSarcina);

                            if (sarcinaNoua.DataFinalizare > DateActions.ConvertToIntFormat(dataFinalizareProiect)
                                || sarcinaNoua.DataDeFinalizat > DateActions.ConvertToIntFormat(dataFinalizareProiect))
                                break;

                            //move no next.
                            dataCreareSarcina = DateActions.ConvertToDateTime((int)sarcinaNoua.DataFinalizare).AddDays(1);
                            counterSarcini++;

                            listaSarcini.Add(sarcinaNoua);

                        }

                        //Salvare sarcini create
                        _context.Sarcini.AddRange(listaSarcini);
                        try
                        {
                            await _context.SaveChangesAsync();
                            _context.ChangeTracker.Clear();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eroare la salvare perioadelor de lucru \n{ex.Message}");
                            Environment.Exit(0);
                        }

                        //Creare Perioade de lucru
                        List<PerioadeDeLucru> listaPerioadeLucru = new List<PerioadeDeLucru>();
                        foreach (Sarcini s in listaSarcini)
                        {
                            List<PerioadeDeLucru> temp = newPerioadeDeLucru(s);
                            listaPerioadeLucru.AddRange(temp);
                            temp.Clear();

                        }

                        //Salvare perioade de lucru
                        _context.PerioadeDeLucru.AddRange(listaPerioadeLucru);
                        try
                        {
                            await _context.SaveChangesAsync();
                            _context.ChangeTracker.Clear();                    
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eroare la salvare perioadelor de lucru \n{ex.Message}");
                            Environment.Exit(0);
                        }

                    }
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("Terminat Inserare");
        }


        private static async Task adaugaMembriInEchipa(Proiecte proiect, List<Angajati> membri)
        {

            foreach(Angajati ang in membri)
            {
                MembriDezvoltare membru = new MembriDezvoltare()
                {
                    DataIntrare = proiect.DataIncepere,
                    DataIesire = proiect.DataFinalizare,
                    Status = "inactiv",
                    MarcaAngajat = ang.MarcaAngajat,
                    NrProiect = proiect.NrProiect,

                };
                await _context.MembriDezvoltare.AddAsync(membru);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Eroare la salvare membri echipa \n{ex.Message}");
                Environment.Exit(0);
            }
            return ;
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


        private static Sarcini newSarcina(int NrSarcina, int IdFunctionalitate, int MarcaAngajat, DateTime DataCreare, string GradDificultate = "", string GradUrgenta = "")
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


            if(string.IsNullOrWhiteSpace(GradDificultate))
            {
                List<string> temp = ListaGradeDificultate.Keys.ToList();
                GradDificultate = faker.PickRandom(temp);
            }

            if (string.IsNullOrWhiteSpace(GradUrgenta))
            {
                List<string> temp = ListaGradeUrgenta.Keys.ToList();
                GradUrgenta = faker.PickRandom(temp);
            }

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
            int minZileLucrate = MatriceDateDeFinalizare[idGradDificultate-1][idGradUrgenta-1]["min"];
            int maxZileLucrate = MatriceDateDeFinalizare[idGradDificultate-1][idGradUrgenta-1]["max"];

            // Se randomizeaza un nr intre acel min si max
            int zileAdaugate_dataDeFinalizat = faker.Random.Number(minZileLucrate, maxZileLucrate);

            //Se adauga nr random la data de creare.
            DateTime DataDeFinalizat = DataCreare.AddDays(zileAdaugate_dataDeFinalizat);
            int dataDeFinalizat = DateActions.ConvertToIntFormat(DataDeFinalizat);

            int dataFinalizare = 0;
            while (dataCreare > dataFinalizare)
            {
                // Se randomizeaza un nr de zile ce se va scade sau adauga din data de finalizat 
                int zile_dataFinalizare = faker.Random.Number(-3, 3);

                //Se adauga / scad zilele randomizate
                DateTime DataFinalizare = DataDeFinalizat.AddDays(-zile_dataFinalizare);
                dataFinalizare = DateActions.ConvertToIntFormat(DataFinalizare);
            }

            if (dataCreare > dataFinalizare )
                throw new Exception("Data Creare > Data Finalizare");

            Sarcini sarcina = new Sarcini()
            {
                Denumire = denumire,
                Descriere = descriere,
                DataCreare = dataCreare,
                DataFinalizare = dataFinalizare,
                MarcaAngajat = marcaAngajat,
                DataDeFinalizat = dataDeFinalizat,
                IDFunctionalitate = IdFunctionalitate,
                IDGradDificultate = idGradDificultate,
                IDGradUrgentaSarcina = idGradUrgenta,
                CalificativDePerformanta = calificativ
            };

            return sarcina;
        }

        private static List<PerioadeDeLucru> newPerioadeDeLucru(Sarcini sarcina)
        {
            var faker = new Faker("ro");
            // Console.WriteLine($"In Perioade");

            List<PerioadeDeLucru> listaPerioade = new List<PerioadeDeLucru>() ;
            DateTime dataCurentaInregistrare = DateActions.ConvertToDateTime(sarcina.DataCreare);
            DateTime dataFinalizarePerioada = DateActions.ConvertToDateTime((int)sarcina.DataFinalizare) ;
            //Console.WriteLine($"In Perioade2");
            while (dataCurentaInregistrare <= dataFinalizarePerioada)
            {
                //Console.WriteLine($"In Perioade loop");
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
                dataCurentaInregistrare = dataCurentaInregistrare.AddDays(1);
            }
            //Console.WriteLine("Creare Perioade");
            return listaPerioade;
        }

    }
}
