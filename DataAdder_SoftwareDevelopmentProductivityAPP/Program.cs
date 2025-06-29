﻿
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
using System.Runtime.Intrinsics.Arm;
using System.Linq;

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


            //Dictionary<string,Dictionary<string, int>> nrSarciniPerAngajatPerPost = new Dictionary<string, Dictionary<string, int>>
            //{
            //    ["Design"] = new Dictionary<string, int>
            //    {
            //        ["min"] = 1,
            //        ["max"] = 2, 
            //    },
            //    ["Dev"] = new Dictionary<string, int>
            //    {
            //        ["min"] = 2,
            //        ["max"] = 3,
            //    },
            //    ["Tester"] = new Dictionary<string, int>
            //    {
            //        ["min"] = 2,
            //        ["max"] = 3,
            //    },
            //};


            //Start2(20220101,
            //    20221220,
            //    20221228,
            //    2,
            //    1,
            //    2,
            //    0,
            //    0,
            //    3,
            //    2
            //    );
            //ModifyAngajati(65);
            //CreateEmployeeAccounts(109);
            //ModifyFirme(5);
            //ModifySarcini();
            //AdaugaProbleme();
            //ModifyDocumenteInterneDescriere();

            //FixSarciniFaraPerioadeDeLucru();
            //RemoveSarciniAndPerioadeForManagersAndArhitects();
            //addSarcinaIntreLuni();
            //ReplaceEmployee();

            //addTaskToAlreadyFinishedProject(7,20230601,1);
            Console.ReadKey();

       }

        private static async void addTaskToAlreadyFinishedProject(int marcaAngajat,int dataCreareSarcina,int zileAdaugate_PanaLaFinalizare)
        {
            Faker faker = new Faker();
            MembriDezvoltare membru = await _context.MembriDezvoltare
                .Include(md => md.Angajat)
                    .ThenInclude(a => a.Post)
                .Include(md => md.Proiect)
                .FirstOrDefaultAsync(md => md.MarcaAngajat == marcaAngajat
                && dataCreareSarcina >= md.DataIntrare && dataCreareSarcina <= md.DataIesire);

            if(membru == null)
            {
                Console.WriteLine("Angajatul nu face parte din nici un proiect in perioada de creare a sarcinii.");
                return;
            }

            Angajati angajat = membru.Angajat;
            if(angajat.Post.Denumire.Equals("Manager Proiect") || angajat.Post.Denumire.Equals("Arhitect Software"))
            {
                Console.WriteLine("Angajatul nu poate avea sarcini. (Este manager sau arhitect)");
                return;
            }

            List<int> funcList = await _context.Functionalitati
                .Where(f => f.NrProiect == membru.NrProiect)
                .Select(f => f.IDFunctionalitate)
                .ToListAsync();

            Proiecte proiect = membru.Proiect;
            int idFunct = faker.PickRandom(funcList);
            int nrSarcina = _context.Sarcini.Count() +1;
            DateTime dataCreareSrc = DateTime.Now;

            try
            {
                dataCreareSrc = DateActions.ConvertToDateTime(dataCreareSarcina);

            }
            catch
            {
                Console.WriteLine("Data nu poate fi creata deoarece nu exista.");
                return;
            }

            DateTime dataFinalizareSrc = dataCreareSrc.AddDays(zileAdaugate_PanaLaFinalizare);
            Sarcini sarcina = newSarcina(nrSarcina,idFunct,marcaAngajat,dataCreareSrc);


            sarcina.DataFinalizare = DateActions.ConvertToIntFormat(dataFinalizareSrc);

            if(sarcina.DataFinalizare > proiect.DataFinalizare )
            {
                Console.WriteLine("Data de finalizare a sarcinii este mai mare decat data de finalizare a proiectului");
                return;
            }


            _context.Sarcini.Add(sarcina);

            try
            {
               await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }


            List<PerioadeDeLucru> perioade = newPerioadeDeLucru(sarcina);

            _context.PerioadeDeLucru.AddRange(perioade);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            Console.WriteLine("Finished");
        }

        private static async void ReplaceEmployee()
        {
            int id_MembruDezvoltare = 172;// 172/204
            int idMembruCuCareInlocuiesc = 7;
            int membruInclocuit = 68;

            //Membru Dezvoltare

            MembriDezvoltare membru = await _context.MembriDezvoltare
                .Include(md => md.Proiect)
                .ThenInclude(p => p.Functionalitati)
                .ThenInclude(f => f.Sarcini)
                .ThenInclude(s => s.PerioadeDeLucru)
                .FirstOrDefaultAsync(md => md.IDMembruDezvoltare == id_MembruDezvoltare);

            membru.MarcaAngajat = idMembruCuCareInlocuiesc;

            _context.MembriDezvoltare.Entry(membru).State = EntityState.Modified;
            //Sarcini

            foreach(Functionalitati f in membru.Proiect.Functionalitati)
                foreach(Sarcini sarcina in f.Sarcini.Where(s => s.MarcaAngajat == membruInclocuit))
                {
                    sarcina.MarcaAngajat = idMembruCuCareInlocuiesc;

                    _context.Sarcini.Entry(sarcina).State = EntityState.Modified;


                    foreach (PerioadeDeLucru perioada in sarcina.PerioadeDeLucru)
                    {
                        perioada.MarcaAngajat = idMembruCuCareInlocuiesc;

                        _context.PerioadeDeLucru.Entry(perioada).State = EntityState.Modified;
                    }
                }


            try
            {
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex}");

            }

            Console.WriteLine("Finished");
        }

        private static async void RemoveSarciniAndPerioadeForManagersAndArhitects()
        {
            List<Sarcini> sarcini = await _context.Sarcini
                .Include(s => s.Angajat)
                .Where(s => s.Angajat.IDPost == 3 || s.Angajat.IDPost == 1)
                .ToListAsync();

            Console.WriteLine($"Sarcini: {sarcini.Count}");

            List<PerioadeDeLucru> perioade = new List<PerioadeDeLucru>();

            foreach(Sarcini sarc in sarcini)
            {
                List<PerioadeDeLucru> temp = await _context.PerioadeDeLucru
                    .Where(s => s.IDSarcina == sarc.IDSarcina)
                    .ToListAsync();

                List<ProblemeDeRezolvare> probleme = await _context.ProblemeDeRezolvare
                    .Where(s => s.IDSarcina == sarc.IDSarcina)
                    .ToListAsync();


                _context.PerioadeDeLucru.RemoveRange(temp);
                _context.ProblemeDeRezolvare.RemoveRange(probleme);
                _context.Sarcini.Remove(sarc);

                await _context.SaveChangesAsync();
            }


            Console.WriteLine("Finished");
        }

        private static async void FixSarciniFaraPerioadeDeLucru()
        {
            //int nrProiect = 32;
            //int idSarcina = 3944;

           List<Sarcini> sarcini = await _context.Sarcini
                //.Include(s => s.Functionalitate).ThenInclude(f => f.Proiect)
                .Where(s => s.DataFinalizare != null && (s.PerioadeDeLucru == null || s.PerioadeDeLucru.Count == 0))
                .ToListAsync();

            //List<PerioadeDeLucru> perioadeProiect = await _context.PerioadeDeLucru
            //    .Include( p => p.Sarcina)
            //        .ThenInclude(s =>s.Functionalitate)
            //        .ThenInclude(f => f.Proiect)
            //    .Where(p => p.Sarcina.Functionalitate.NrProiect == nrProiect)

            //    .ToListAsync();

            //List<PerioadeDeLucru> perioadeSarcina = await _context.PerioadeDeLucru
            //    .Include(p => p.Sarcina)
            //        .ThenInclude(s => s.Functionalitate)
            //        .ThenInclude(f => f.Proiect)
            //    .Where(p => p.IDSarcina == idSarcina)
            //    .ToListAsync();

            //Console.WriteLine($"Perioade proiect nr {nrProiect}: {perioadeProiect.Count}");
            //Console.WriteLine($"Perioade sarcina nr {idSarcina}: {perioadeSarcina.Count}");
            Console.WriteLine($"Sarcini de modificat: {sarcini.Count}");
            //foreach (Sarcini s in sarcini)
            //{
            //    //Console.WriteLine($"{s.IDSarcina}.{s.Denumire} - {s.Functionalitate.IDFunctionalitate}.{s.Functionalitate.Denumire} - {s.Functionalitate.Proiect.NrProiect}.{s.Functionalitate.Proiect.Denumire}");
            //    List<PerioadeDeLucru> temp = newPerioadeDeLucru(s);

            //    await _context.PerioadeDeLucru.AddRangeAsync(temp);

            //}

            //await _context.SaveChangesAsync();
            //Console.WriteLine("Finished");

        }

        private static async void ModifyDocumenteInterneDescriere()
        {
            int idTipDoc = 5;
            string descriere = "Contine pentru mai multi Angajati in maxim 4 ani: \n- productivitatea \n- factorul de respectare a termenelor de finalizare a sarcinilor.";

            TipuriDocumenteInterne tip = await _context.TipuriDocumenteInterne.FirstOrDefaultAsync(tid => tid.IDTipDocument == idTipDoc);
            tip.Descriere = descriere;

            _context.TipuriDocumenteInterne.Entry(tip).State = EntityState.Modified;

            //await _context.SaveChangesAsync();
        
        }


        private static async void addSarcinaIntreLuni()
        {
            Angajati angajat = _context.Angajati.FirstOrDefault(a => a.MarcaAngajat == 5);
            int nrSarcina = _context.Sarcini.OrderBy(s => s.IDSarcina).Select(s => s.IDSarcina).LastOrDefault();

            int dataIncepere = 20240229;

            nrSarcina++;
            Sarcini sarcina = newSarcina(nrSarcina,25,5,DateActions.ConvertToDateTime(dataIncepere),"Usoara","Maxima");
            _context.Sarcini.Add(sarcina);
            await _context.SaveChangesAsync();

            List<PerioadeDeLucru> perioade= newPerioadeDeLucru(sarcina);
            await _context.PerioadeDeLucru.AddRangeAsync(perioade);
            await _context.SaveChangesAsync();

            Console.WriteLine("Finished");
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
            int nrEchipeNoi = 0,
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
            int nrFirmeNoi = 1,
            int Proiecte_PerFirma = 1,
            int nrManageriNoi = 0,
            int nrEchipeNoi = 0,
            int Dezvoltatori_PerEchipa = 3,
            int Designeri_PerEchipa = 2,
            int Testeri_PerEchipa = 2)
        {
            var faker = new Faker("ro");

            int nrProiect = 0;

            for (int i = 1; i<= nrManageriNoi;i++)
            {
                Console.WriteLine("Creating new Managers");
                Angajati angajatNou = newAngajat("Manager");
                await _context.Angajati.AddAsync(angajatNou);
                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Created manager {i + 1}");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Eroare la salvare angajat nou Dev \n{ex.Message}");
                    Environment.Exit(0);
                }    
            }

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
                List<Proiecte> listaProiecteNoi = new List<Proiecte>();
                
                for (int p = 1; p <= Proiecte_PerFirma; p++)
                {
                    nrProiect = _context.Proiecte.Count() + 1;


                    //creare Proiecte
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

                    listaProiecteNoi.Add(proiectNou);
                }


                foreach (Proiecte proiectNou in listaProiecteNoi)
                {
                    DateTime dataIncepereProiect = DateActions.ConvertToDateTime(dataIncepere_Proiect);
                    DateTime dataFinalizareProiect = DateActions.ConvertToDateTime(dataFinalizare_Proiect);
                    DateTime dataDeFinalizatProiect = DateActions.ConvertToDateTime(dataDeTerminat_Proiect);

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
                    List<Angajati> membriDezvoltare = new List<Angajati>();
                    int devAdded = 0;
                    int designerAdded = 0;
                    int testerAdded = 0;
                    int arhitectAdded = 0;

                    int nrMembriEchipa = (Dezvoltatori_PerEchipa + Designeri_PerEchipa + Testeri_PerEchipa + 1);

                    do
                    {
                        Console.WriteLine("LOOP");
                        //Creare angajati in caz ca nu sunt destui liberi
                        if (nrEchipeNoi == 0)
                        {
                            
                            //Creare angajati noi
                            while (developeri.Count < Dezvoltatori_PerEchipa)
                            {
                                Angajati angajatNou = newAngajat("Dev");
                                await _context.Angajati.AddAsync(angajatNou);
                                try
                                {
                                    await _context.SaveChangesAsync();
                                    developeri.Add(angajatNou);
                                    Console.WriteLine($"Created new employee dev");

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Eroare la salvare angajat nou Dev \n{ex.Message}");
                                    Environment.Exit(0);
                                }
                            }
                            while (designeri.Count < Designeri_PerEchipa)
                            {
                                Angajati angajatNou = newAngajat("Designer");
                                await _context.Angajati.AddAsync(angajatNou);
                                try
                                {
                                    await _context.SaveChangesAsync();
                                    designeri.Add(angajatNou);
                                    Console.WriteLine($"Created new employee Designer");

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Eroare la salvare angajat nou Designer \n{ex.Message}");
                                    Environment.Exit(0);
                                }

                            }
                            while (testeri.Count < Testeri_PerEchipa)
                            {
                                Angajati angajatNou = newAngajat("Tester");
                                await _context.Angajati.AddAsync(angajatNou);
                                try
                                {
                                    await _context.SaveChangesAsync();
                                    testeri.Add(angajatNou);
                                    Console.WriteLine($"Created new employee Tester");

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Eroare la salvare angajat nou Tester \n{ex.Message}");
                                    Environment.Exit(0);
                                }

                            }
                            while (arhitecti.Count < 1)
                            {
                                Angajati angajatNou = newAngajat("Arhitect");

                                await _context.Angajati.AddAsync(angajatNou);
                                try
                                {
                                    await _context.SaveChangesAsync();
                                    Console.WriteLine($"Created new employee Arhitect");

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Eroare la salvare angajat nou Arhitect \n{ex.Message}");
                                    Environment.Exit(0);
                                }
                                arhitecti.Add(angajatNou);
                            }
                        }
                        else
                        {
                            developeri = new List<Angajati>();
                            designeri = new List<Angajati>();
                            testeri = new List<Angajati>();
                            arhitecti = new List<Angajati>();

                            Console.WriteLine("Creating new teams");
                            for (int i = 0; i < nrEchipeNoi; i++)
                            {
                                Console.WriteLine($"Creating team {i+1}");
                                for (int j = 0; j < Dezvoltatori_PerEchipa; j++)
                                {
                                    Angajati angajatNou = newAngajat("Dev");
                                    await _context.Angajati.AddAsync(angajatNou);
                                    try
                                    {
                                        await _context.SaveChangesAsync();
                                        developeri.Add(angajatNou);
                                        Console.WriteLine($"Created dev {j + 1}");

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Eroare la salvare angajat nou Dev \n{ex.Message}");
                                        Environment.Exit(0);
                                    }
                                }

                                for (int j = 0; j < Designeri_PerEchipa; j++)
                                {
                                    Angajati angajatNou = newAngajat("Designer");
                                    await _context.Angajati.AddAsync(angajatNou);
                                    try
                                    {
                                        await _context.SaveChangesAsync();
                                        designeri.Add(angajatNou);
                                        Console.WriteLine($"Created designer {j + 1}");

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Eroare la salvare angajat nou Designer \n{ex.Message}");
                                        Environment.Exit(0);
                                    }

                                }
                                for (int j = 0; j < Testeri_PerEchipa; j++)
                                {
                                    Angajati angajatNou = newAngajat("Tester");
                                    await _context.Angajati.AddAsync(angajatNou);
                                    try
                                    {
                                        await _context.SaveChangesAsync();
                                        testeri.Add(angajatNou);
                                        Console.WriteLine($"Created tester {j + 1}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Eroare la salvare angajat nou Tester \n{ex.Message}");
                                        Environment.Exit(0);
                                    }

                                }
                                for (int j = 0; j < 1; j++)
                                {
                                    Angajati angajatNou = newAngajat("Arhitect");

                                    await _context.Angajati.AddAsync(angajatNou);
                                    try
                                    {
                                        await _context.SaveChangesAsync();
                                        arhitecti.Add(angajatNou);
                                        Console.WriteLine($"Created arhitect {j + 1}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Eroare la salvare angajat nou Arhitect \n{ex.Message}");
                                        Environment.Exit(0);
                                    }

                                }
                            }
                        }

                        //Creare echipe noi
                        nrEchipeNoi = 0;
                        // Adaugare angajati in echipa
                        if(devAdded < Dezvoltatori_PerEchipa)
                            for (int i = 0; i < Dezvoltatori_PerEchipa; i++)
                            {
                                Angajati angDeAdaugat = faker.PickRandom(developeri);
                                developeri.Remove(angDeAdaugat);

                                Angajati temp = membriDezvoltare.FirstOrDefault(a => a.MarcaAngajat == angDeAdaugat.MarcaAngajat);                                                 
                                if (temp == null)
                                {
                                    MembriDezvoltare md_DB = await _context.MembriDezvoltare.FirstOrDefaultAsync(md => angDeAdaugat.MarcaAngajat == md.MarcaAngajat &&
                                    ((md.DataIntrare >= proiectNou.DataIncepere && md.DataIesire <= proiectNou.DataFinalizare) ||
                                    (md.DataIntrare <= proiectNou.DataIncepere && md.DataIesire>= proiectNou.DataIncepere && md.DataIesire <= proiectNou.DataFinalizare) ||
                                    (md.DataIntrare >= proiectNou.DataIncepere && md.DataIntrare <= proiectNou.DataFinalizare && md.DataIesire >= proiectNou.DataFinalizare))
                                    );
                                    if(md_DB == null)
                                    {
                                        membriDezvoltare.Add(angDeAdaugat);
                                        devAdded++;
                                    }
                                }
                            }


                        if (designerAdded < Designeri_PerEchipa)
                            for (int i = 0; i < Designeri_PerEchipa; i++)
                            {
                                Angajati angDeAdaugat = faker.PickRandom(designeri);
                                designeri.Remove(angDeAdaugat);

                                Angajati temp = membriDezvoltare.FirstOrDefault(a => a.MarcaAngajat == angDeAdaugat.MarcaAngajat);
                                if (temp == null)
                                {
                                    MembriDezvoltare md_DB = await _context.MembriDezvoltare.FirstOrDefaultAsync(md => angDeAdaugat.MarcaAngajat == md.MarcaAngajat &&
                                    ((md.DataIntrare >= proiectNou.DataIncepere && md.DataIesire <= proiectNou.DataFinalizare) ||
                                    (md.DataIntrare <= proiectNou.DataIncepere && md.DataIesire >= proiectNou.DataIncepere && md.DataIesire <= proiectNou.DataFinalizare) ||
                                    (md.DataIntrare >= proiectNou.DataIncepere && md.DataIntrare <= proiectNou.DataFinalizare && md.DataIesire >= proiectNou.DataFinalizare))
                                    );
                                    if (md_DB == null)
                                    {
                                        membriDezvoltare.Add(angDeAdaugat);
                                        designerAdded++;
                                    }
                                }
                            }
                        if (testerAdded < Testeri_PerEchipa)
                            for (int i = 0; i < Testeri_PerEchipa; i++)
                            {
                                Angajati angDeAdaugat = faker.PickRandom(testeri);
                                testeri.Remove(angDeAdaugat);

                                Angajati temp = membriDezvoltare.FirstOrDefault(a => a.MarcaAngajat == angDeAdaugat.MarcaAngajat);
                                if (temp == null)
                                {
                                    MembriDezvoltare md_DB = await _context.MembriDezvoltare.FirstOrDefaultAsync(md => angDeAdaugat.MarcaAngajat == md.MarcaAngajat &&
                                    ((md.DataIntrare >= proiectNou.DataIncepere && md.DataIesire <= proiectNou.DataFinalizare) ||
                                    (md.DataIntrare <= proiectNou.DataIncepere && md.DataIesire >= proiectNou.DataIncepere && md.DataIesire <= proiectNou.DataFinalizare) ||
                                    (md.DataIntrare >= proiectNou.DataIncepere && md.DataIntrare <= proiectNou.DataFinalizare && md.DataIesire >= proiectNou.DataFinalizare))
                                    );
                                    if (md_DB == null)
                                    {
                                        membriDezvoltare.Add(angDeAdaugat);
                                        testerAdded++;
                                    }
                                }
                            }

                        if (arhitectAdded < 1)
                            for (int i = 0; i < 1; i++)
                            {
                                Angajati angDeAdaugat = faker.PickRandom(arhitecti);
                                arhitecti.Remove(angDeAdaugat);

                                Angajati temp = membriDezvoltare.FirstOrDefault(a => a.MarcaAngajat == angDeAdaugat.MarcaAngajat);
                                if (temp == null)
                                {
                                    MembriDezvoltare md_DB = await _context.MembriDezvoltare.FirstOrDefaultAsync(md => angDeAdaugat.MarcaAngajat == md.MarcaAngajat &&
                                    ((md.DataIntrare >= proiectNou.DataIncepere && md.DataIesire <= proiectNou.DataFinalizare) ||
                                    (md.DataIntrare <= proiectNou.DataIncepere && md.DataIesire >= proiectNou.DataIncepere && md.DataIesire <= proiectNou.DataFinalizare) ||
                                    (md.DataIntrare >= proiectNou.DataIncepere && md.DataIntrare <= proiectNou.DataFinalizare && md.DataIesire >= proiectNou.DataFinalizare))
                                    );
                                    if (md_DB == null)
                                    {
                                        membriDezvoltare.Add(angDeAdaugat);
                                        arhitectAdded++;
                                    }
                                }
                            }
                    } while (membriDezvoltare.Count < nrMembriEchipa);

                    membriDezvoltare.Add(faker.PickRandom(manageri));
                    await adaugaMembriInEchipa(proiectNou, membriDezvoltare);


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

                    List<int> ListaMarcaAngajati = membriDezvoltare
                        .Where(md => md.IDPost != 3 || md.IDPost != 1)
                        .Select(a => a.MarcaAngajat).ToList();
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


        private static async void AdaugaProbleme()
        {
            Faker faker = new Faker();  
            List<int> listaSansa = new List<int>()
            {
                1,2,3,4,5,6,7,8,9,10

            };

            List<Proiecte> proiecte = await _context.Proiecte
                .Where(p => p.DataFinalizare != null)
                .ToListAsync();

            int nrProblema = await _context.ProblemeDeRezolvare.CountAsync();

            Console.WriteLine(proiecte.Count());

            foreach(Proiecte proiect in proiecte)
            {
                List<Functionalitati> functionalitati = await _context.Functionalitati
                    .Where(f=> f.NrProiect == proiect.NrProiect)
                    .ToListAsync();

                Angajati arhitect = await _context.MembriDezvoltare
                    .Include(md => md.Angajat)
                    .Where(md => md.NrProiect == proiect.NrProiect && md.Angajat.IDPost == 3)
                    .Select(md => md.Angajat)
                    .FirstOrDefaultAsync();

                if (arhitect == null)
                    break;

                foreach(Functionalitati func in functionalitati)
                {
                    List<Sarcini> listaSarcini = await _context.Sarcini
                        .Where(s => s.IDFunctionalitate == func.IDFunctionalitate)  
                        .ToListAsync();
                    
                    foreach(Sarcini sarcina in listaSarcini)
                    {
                        if(faker.PickRandom(listaSansa) > 8)
                        {
                            nrProblema++;
                            DateTime dataInregistrare = DateActions.ConvertToDateTime(sarcina.DataCreare);
                            DateTime dataRezolvare = dataInregistrare.AddDays(1);

                            if (DateActions.ConvertToDateTime((int)sarcina.DataFinalizare) < dataRezolvare)
                                dataRezolvare.AddDays(-1);

                            ProblemeDeRezolvare problema = newProblema(nrProblema, sarcina.IDSarcina, arhitect.MarcaAngajat, dataInregistrare, dataRezolvare);


                            await _context.ProblemeDeRezolvare.AddAsync(problema);
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
        

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

        private static async void ModifyAngajati(int nrAngajatiModificati)
        {
            List<Angajati> listaAngajati = await _context.Angajati.Where(a => a.MarcaAngajat <= nrAngajatiModificati).ToListAsync();


            foreach(Angajati angajat in listaAngajati)
            {
                Angajati modificariAngajat = newAngajat(getPostString(angajat.IDPost));

                angajat.NumeSiPrenume = modificariAngajat.NumeSiPrenume;
                angajat.Email = modificariAngajat.Email;
                angajat.NumarDeTelefon = modificariAngajat.NumarDeTelefon;
                angajat.Localitate = modificariAngajat.Localitate;
                angajat.Strada_si_numar = modificariAngajat.Strada_si_numar;
                angajat.CNP = modificariAngajat.CNP;
                angajat.SerieCIBI = modificariAngajat.SerieCIBI;

                _context.Entry(angajat).State = EntityState.Modified;

            }

            await _context.SaveChangesAsync();
        }

        private static async void ModifyFirme(int nrFirmeModificate)
        {
            List<Firme> listaFirme = await _context.Firme.Where(a => a.CODFirma <= nrFirmeModificate).ToListAsync();


            foreach (Firme firma in listaFirme)
            {
                Firme modificariFirma = newFirma();

                //firma.Denumire = modificariFirma.Denumire;
                firma.Denumire = $"Firma {firma.CODFirma}";
                firma.CUI = modificariFirma.CUI;
                //firma.Email = modificariFirma.Email;
                firma.NumarDeTelefon = modificariFirma.NumarDeTelefon;
                //firma.Tara = modificariFirma.Tara;
                //firma.Localitate = modificariFirma.Localitate;
                //firma.Strada_si_numar = modificariFirma.Strada_si_numar;

                _context.Entry(firma).State = EntityState.Modified;

            }

            await _context.SaveChangesAsync();
        }

        private static async void ModifySarcini()
        {
            List<Sarcini> listaSarcini = await _context.Sarcini
                .Where(s => s.CalificativDePerformanta != null)
                .ToListAsync();


            foreach(Sarcini sarcina in listaSarcini)
            {
                sarcina.CalificativDePerformanta = Math.Round((decimal)sarcina.CalificativDePerformanta, 1, MidpointRounding.AwayFromZero);

                _context.Sarcini.Entry(sarcina).State = EntityState.Modified;              
            }
            await _context.SaveChangesAsync();
        }
        
        private static ProblemeDeRezolvare newProblema(int NrProblema, int idSarcina, int marcaArhitect,DateTime DataInregistrare, DateTime DataRezolvare )
        {
            Faker faker = new Faker();
            List<int> gradeUrg = new List<int>() { 1,2,3};

            string titlu = $"Problema {NrProblema}";
            string descriere = $"Descriere problema {NrProblema}";
            string solutie = $"Solutie problema {NrProblema}";
            int dataInregistrare = DateActions.ConvertToIntFormat(DataInregistrare);
            int dataRezolvare = DateActions.ConvertToIntFormat(DataRezolvare);
            string status = "Rezolvata";
            int idGradUrgenta = faker.PickRandom(gradeUrg);

            ProblemeDeRezolvare problema = new ProblemeDeRezolvare()
            {
                Titlu = titlu,
                Descriere = descriere,
                Solutie = solutie,
                DataIntregistrare = dataInregistrare,
                DataRezolvare = dataRezolvare,
                Status = status,
                IDSarcina = idSarcina,
                MarcaAngajat = marcaArhitect,
                IDGradUrgentaProblema = idGradUrgenta,
            };

            return problema;           
        }

        private static async void CreateEmployeeAccounts(int nrAngajatiModificati)
        {
            List<Angajati> listaAngajati = await _context.Angajati
                .Include(a => a.Cont)
                .Include(a => a.Post)
                .Where(a => a.MarcaAngajat <= nrAngajatiModificati)
                .ToListAsync();


            foreach (Angajati angajat in listaAngajati)
            {
                if(angajat.Cont == null)
                {
                    int idRol = 2;
                    string usernameCont = $"ang{angajat.MarcaAngajat}";

                    if(angajat.Post.IDPost == 1)
                    {
                        idRol = 1;
                        usernameCont = $"m{angajat.MarcaAngajat}";
                    }

                    if (angajat.Post.IDPost == 3)
                    {
                        idRol = 3;
                        usernameCont = $"arh{angajat.MarcaAngajat}";
                    }

                    Conturi cont = new Conturi()
                    {
                        Username = $"{usernameCont}",
                        Password = "1234",
                        IDRol = idRol,
                        MarcaAngajat = angajat.MarcaAngajat
                    };

                    await _context.Conturi.AddAsync(cont);
                }
                else
                {
                    Conturi contAng = angajat.Cont;

                    string usernameCont = $"ang{angajat.MarcaAngajat}";

                    if (angajat.Post.IDPost == 1)
                    {
                        usernameCont = $"mg{angajat.MarcaAngajat}";
                    }


                    if (angajat.Post.IDPost == 3)
                    {
                        usernameCont = $"arh{angajat.MarcaAngajat}";
                    }

                    contAng.Username = $"{usernameCont}";
                    contAng.Password = "1234";

                    _context.Conturi.Entry(contAng).State = EntityState.Modified;
                }

            }

            await _context.SaveChangesAsync();
        }

        private static Angajati newAngajat(string Post)
        {
            var faker = new Faker("ro");

            List<string> listaSeriiBucuresti = new List<string>() { "DP", "DR", "DT", "DX", "RD", "RR", "RT", "RX", "RK", "RZ" };

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

            string serieCIBI = faker.PickRandom(listaSeriiBucuresti);

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
            decimal calificativ = Math.Round(faker.Random.Decimal((decimal)0.5, (decimal)1.0),1,MidpointRounding.AwayFromZero);

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

        private static string getPostString(int postId)
        {
            string postString;
            switch (postId)
            {
                case 1:
                    postString = "Manager";
                    break;

                case 2:
                    postString = "Designer";
                    break;

                case 3:
                    postString = "Arhitect";
                    break;

                case 4:
                    postString = "Dev";
                    break;

                case 5:
                    postString = "Tester";
                    break;

                default:
                    throw new Exception("Post invalid");
                    break;
            }
            return postString;
        }
    
    }
}
