using System;
using System.Collections.Generic;
using ClassLibrary_SoftwareDevelopmentProductivityAPP.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAdder_SoftwareDevelopmentProductivityAPP;

public partial class DBContext : DbContext
{
    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DBContext> options)
        : base(options)
    {
    }


    public virtual DbSet<Angajati> Angajati { get; set; }


    public virtual DbSet<Conturi> Conturi { get; set; }

    public virtual DbSet<Departamente> Departamente { get; set; }

    public virtual DbSet<Firme> Firme { get; set; }

    public virtual DbSet<Functionalitati> Functionalitati { get; set; }

    public virtual DbSet<GradeDeDificultate> GradeDeDificultate { get; set; }

    public virtual DbSet<GradeUrgentaProbleme> GradeUrgentaProbleme { get; set; }

    public virtual DbSet<GradeUrgentaSarcini> GradeUrgentaSarcini { get; set; }

    public virtual DbSet<MembriDezvoltare> MembriDezvoltare { get; set; }

    public virtual DbSet<PerioadeDeLucru> PerioadeDeLucru { get; set; }

    public virtual DbSet<Posturi> Posturi { get; set; }

    public virtual DbSet<ProblemeDeRezolvare> ProblemeDeRezolvare { get; set; }

    public virtual DbSet<Proiecte> Proiecte { get; set; }

    public virtual DbSet<RapoarteExterne> RapoarteExterne { get; set; }

    public virtual DbSet<DocumenteInterne> DocumenteInterne { get; set; }

    public virtual DbSet<Roluri> Roluri { get; set; }

    public virtual DbSet<Sarcini> Sarcini { get; set; }

    public virtual DbSet<Subtask> Subtasks { get; set; }

    public virtual DbSet<TipuriRapoarte> TipuriRapoarte { get; set; }
    public virtual DbSet<TipuriDocumenteInterne> TipuriDocumenteInterne { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Angajati>(entity =>
        {
            entity.HasKey(e => e.MarcaAngajat);

            entity.ToTable("Angajati");

            entity.Property(e => e.MarcaAngajat).HasColumnName("Marca_Angajat");
            entity.Property(e => e.Localitate).HasMaxLength(50);
            entity.Property(e => e.Strada_si_numar).HasMaxLength(100);
            entity.Property(e => e.CNP)
                .HasMaxLength(13)
                .HasColumnName("CNP");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.IDPost).HasColumnName("ID_Post");
            entity.Property(e => e.NumarDeTelefon)
                .HasMaxLength(50)
                .HasColumnName("Numar_de_telefon");
            entity.Property(e => e.NumeSiPrenume)
                .HasMaxLength(50)
                .HasColumnName("Nume_si_Prenume");
            entity.Property(e => e.SerieCIBI)
                .HasMaxLength(2)
                .HasColumnName("Serie_CI_BI");

            entity.HasOne(d => d.Post).WithMany(p => p.Angajati)
                .HasForeignKey(d => d.IDPost)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Angajati_Posturi");
        });

        modelBuilder.Entity<Conturi>(entity =>
        {
            entity.HasKey(e => e.IDCont);

            entity.ToTable("Conturi");

            entity.Property(e => e.IDCont).HasColumnName("ID_Cont");
            entity.Property(e => e.IDRol).HasColumnName("ID_Rol");
            entity.Property(e => e.MarcaAngajat).HasColumnName("Marca_Angajat");
            entity.Property(e => e.Password).HasMaxLength(25);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Rol).WithMany(p => p.Conturi)
                .HasForeignKey(d => d.IDRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conturi_Roluri");

            entity.HasOne(d => d.Angajat).WithOne(p => p.Cont)
                .HasForeignKey<Conturi>(d => d.MarcaAngajat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conturi_Angajati");
        });

        modelBuilder.Entity<Departamente>(entity =>
        {
            entity.HasKey(e => e.IDDepartament);

            entity.ToTable("Departamente");

            entity.Property(e => e.IDDepartament).HasColumnName("ID_Departament");
            entity.Property(e => e.Denumire).HasMaxLength(50);
        });

        modelBuilder.Entity<Firme>(entity =>
        {
            entity.HasKey(e => e.CODFirma);

            entity.ToTable("Firme");

            entity.Property(e => e.CODFirma).HasColumnName("Cod_Firma");
            entity.Property(e => e.Tara)
                .HasMaxLength(50)
                .HasColumnName("Tara");
            entity.Property(e => e.Localitate)
                .HasMaxLength(50)
                .HasColumnName("Localitate");
            entity.Property(e => e.Strada_si_numar)
                .HasMaxLength(100)
                .HasColumnName("Strada_si_Numar");

            entity.Property(e => e.CUI)
                .HasMaxLength(50)
                .HasColumnName("CUI");
            entity.Property(e => e.Denumire).HasMaxLength(75);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.NumarDeTelefon)
                .HasMaxLength(50)
                .HasColumnName("Numar_de_telefon");
        });

        modelBuilder.Entity<Functionalitati>(entity =>
        {
            entity.HasKey(e => e.IDFunctionalitate);

            entity.ToTable("Functionalitati");

            entity.Property(e => e.IDFunctionalitate).HasColumnName("ID_Functionalitate");
            entity.Property(e => e.Denumire).HasMaxLength(50);
            entity.Property(e => e.Descriere);
            entity.Property(e => e.NrProiect).HasColumnName("Nr_Proiect");

            entity.HasOne(d => d.Proiect).WithMany(p => p.Functionalitati)
                .HasForeignKey(d => d.NrProiect)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Functionalitati_Proiecte");
        });

        modelBuilder.Entity<GradeDeDificultate>(entity =>
        {
            entity.HasKey(e => e.IDGradDificultate);

            entity.ToTable("Grade_de_dificultate");

            entity.Property(e => e.IDGradDificultate).HasColumnName("ID_Grad_Dificultate");
            entity.Property(e => e.Denumire).HasMaxLength(15);
            entity.Property(e => e.ModificatorDificultate).HasColumnName("Modificator_dificultate");
        });

        modelBuilder.Entity<GradeUrgentaProbleme>(entity =>
        {
            entity.HasKey(e => e.IDGradUrgentaProblema).HasName("PK_Grade_urgenta");

            entity.ToTable("Grade_Urgenta_Probleme");

            entity.Property(e => e.IDGradUrgentaProblema).HasColumnName("ID_Grad_Urgenta_Problema");
            entity.Property(e => e.Denumire).HasMaxLength(50);
            entity.Property(e => e.GradUrgenta).HasColumnName("Grad_urgenta");
        });

        modelBuilder.Entity<GradeUrgentaSarcini>(entity =>
        {
            entity.HasKey(e => e.IDGradUrgentaSarcina);

            entity.ToTable("Grade_Urgenta_Sarcini");

            entity.Property(e => e.IDGradUrgentaSarcina).HasColumnName("ID_Grad_Urgenta_Sarcina");
            entity.Property(e => e.Denumire).HasMaxLength(50);
            entity.Property(e => e.GradUrgenta).HasColumnName("Grad_Urgenta");


            entity.HasMany(d => d.Sarcini).WithOne(p => p.GradUrgentaSarcina)
                .HasForeignKey(d => d.IDGradUrgentaSarcina)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sarcini_Grade_Urgenta_Sarcini");
        });

        modelBuilder.Entity<MembriDezvoltare>(entity =>
        {
            entity.HasKey(e => e.IDMembruDezvoltare);

            entity.ToTable("Membri_dezvoltare");

            entity.Property(e => e.IDMembruDezvoltare).HasColumnName("ID_MembruDezvoltare");
            entity.Property(e => e.DataIesire).HasColumnName("Data_iesire");
            entity.Property(e => e.DataIntrare).HasColumnName("Data_intrare");
            entity.Property(e => e.MarcaAngajat).HasColumnName("Marca_Angajat");
            entity.Property(e => e.NrProiect).HasColumnName("Nr_Proiect");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Angajat).WithMany(p => p.Membri_Dezvoltare)
                .HasForeignKey(d => d.MarcaAngajat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membri_dezvoltare_Angajati");

            entity.HasOne(d => d.Proiect).WithMany(p => p.MembriDezvoltare)
                .HasForeignKey(d => d.NrProiect)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membri_dezvoltare_Proiecte");
        });

        modelBuilder.Entity<PerioadeDeLucru>(entity =>
        {
            entity.HasKey(e => e.IDPerioadaDeLucru);

            entity.ToTable("Perioade_de_lucru");

            entity.Property(e => e.IDPerioadaDeLucru).HasColumnName("ID_Perioada_de_lucru");
            entity.Property(e => e.IDSarcina).HasColumnName("ID_Sarcina");
            entity.Property(e => e.MarcaAngajat).HasColumnName("Marca_Angajat");
            entity.Property(e => e.OraIncepere).HasColumnName("Ora_incepere");
            entity.Property(e => e.OraTerminare).HasColumnName("Ora_terminare");
            entity.Property(e => e.OreLucrate).HasColumnName("Ore_lucrate");

            entity.HasOne(d => d.Sarcina).WithMany(p => p.PerioadeDeLucru)
                .HasForeignKey(d => d.IDSarcina)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Perioade_de_lucru_Sarcini");

            entity.HasOne(d => d.Angajat).WithMany(p => p.PerioadeDeLucru)
                .HasForeignKey(d => d.MarcaAngajat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Perioade_de_lucru_Angajati");
        });

        modelBuilder.Entity<Posturi>(entity =>
        {
            entity.HasKey(e => e.IDPost);

            entity.ToTable("Posturi");

            entity.Property(e => e.IDPost).HasColumnName("ID_Post");
            entity.Property(e => e.Denumire).HasMaxLength(50);
            entity.Property(e => e.Descriere).HasMaxLength(250);
            entity.Property(e => e.IDDepartament).HasColumnName("ID_Departament");

            entity.HasOne(d => d.Departament).WithMany(p => p.Posturi)
                .HasForeignKey(d => d.IDDepartament)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Posturi_Departamente");
        });

        modelBuilder.Entity<ProblemeDeRezolvare>(entity =>
        {
            entity.HasKey(e => e.IDProblema);

            entity.ToTable("Probleme_de_rezolvare");

            entity.Property(e => e.IDProblema).HasColumnName("ID_Problema");
            entity.Property(e => e.DataIntregistrare).HasColumnName("Data_intregistrare");
            entity.Property(e => e.DataRezolvare).HasColumnName("Data_rezolvare");
            entity.Property(e => e.Descriere);
            entity.Property(e => e.IDGradUrgentaProblema).HasColumnName("ID_Grad_Urgenta_Problema");
            entity.Property(e => e.IDSarcina).HasColumnName("ID_Sarcina");
            entity.Property(e => e.MarcaAngajat).HasColumnName("Marca_Angajat");
            entity.Property(e => e.Solutie);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Titlu).HasMaxLength(50);

            entity.HasOne(d => d.GradUrgentaProblema).WithMany(p => p.ProblemeDeRezolvare)
                .HasForeignKey(d => d.IDGradUrgentaProblema)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Probleme_de_rezolvare_Grade_urgenta");

            entity.HasOne(d => d.Sarcina).WithMany(p => p.ProblemeDeRezolvare)
                .HasForeignKey(d => d.IDSarcina)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Probleme_de_rezolvare_Sarcini");

            entity.HasOne(d => d.Angajat).WithMany(p => p.ProblemeDeRezolvare)
                .HasForeignKey(d => d.MarcaAngajat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Probleme_de_rezolvare_Angajati");
        });

        modelBuilder.Entity<Proiecte>(entity =>
        {
            entity.HasKey(e => e.NrProiect);

            entity.ToTable("Proiecte");

            entity.Property(e => e.NrProiect).HasColumnName("Nr_Proiect");
            entity.Property(e => e.CODFirma).HasColumnName("Cod_Firma");
            entity.Property(e => e.DataDeFinalizat).HasColumnName("Data_de_finalizat");
            entity.Property(e => e.DataFinalizare).HasColumnName("Data_finalizare");
            entity.Property(e => e.DataIncepere).HasColumnName("Data_incepere");
            entity.Property(e => e.Denumire).HasMaxLength(50);
            entity.Property(e => e.Descriere);

            entity.HasOne(d => d.Firma).WithMany(p => p.Proiecte)
                .HasForeignKey(d => d.CODFirma)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Proiecte_Firma");
        });

        modelBuilder.Entity<RapoarteExterne>(entity =>
        {
            entity.HasKey(e => e.NumarRaport);

            entity.ToTable("Rapoarte_Externe");

            entity.Property(e => e.NumarRaport).HasColumnName("Numar_Raport");
            entity.Property(e => e.AdresaFisier)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Adresa_Fisier");
            entity.Property(e => e.CodFirma).HasColumnName("Cod_Firma");
            entity.Property(e => e.DataCreare).HasColumnName("Data_creare");
            entity.Property(e => e.Denumire).HasMaxLength(150);
            entity.Property(e => e.Descriere);
            entity.Property(e => e.IdTipRaport).HasColumnName("ID_Tip_Raport");
            entity.Property(e => e.MarcaAngajatAutor).HasColumnName("Marca_Angajat_Autor");
            entity.Property(e => e.NrProiect).HasColumnName("Nr_Proiect");

            entity.HasOne(d => d.Firma).WithMany(p => p.Rapoarte)
                .HasForeignKey(d => d.CodFirma)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rapoarte_Firma");

            entity.HasOne(d => d.TipRaport).WithMany(p => p.Rapoarte)
                .HasForeignKey(d => d.IdTipRaport)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rapoarte_Tipuri_rapoarte");

            entity.HasOne(d => d.Angajat).WithMany(p => p.RapoarteExterne)
                .HasForeignKey(d => d.MarcaAngajatAutor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rapoarte_Angajati");

            entity.HasOne(d => d.Proiect).WithMany(p => p.Rapoarte)
                .HasForeignKey(d => d.NrProiect)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rapoarte_Proiecte");
        });

        modelBuilder.Entity<DocumenteInterne>(entity =>
        {
            entity.HasKey(e => e.NumarDocument);

            entity.ToTable("Documente_Interne");

            entity.Property(e => e.NumarDocument).HasColumnName("Numar_Document");
            entity.Property(e => e.AdresaFisier)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Adresa_Fisier");
            entity.Property(e => e.DataCreare).HasColumnName("Data_creare");
            entity.Property(e => e.Denumire).HasMaxLength(150);
            entity.Property(e => e.Descriere);
            entity.Property(e => e.IdTipDocument).HasColumnName("ID_Tip_Document");
            entity.Property(e => e.MarcaAngajatAutor).HasColumnName("Marca_Angajat_Autor");


            entity.HasOne(d => d.TipDocument).WithMany(p => p.DocumenteInterne)
                .HasForeignKey(d => d.IdTipDocument)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Documente_Interne_Tipuri_Documente_Interne");

            entity.HasOne(d => d.Angajat).WithMany(p => p.DocumenteInterne)
                .HasForeignKey(d => d.MarcaAngajatAutor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Documente_Interne_Angajati");
        });

        modelBuilder.Entity<Roluri>(entity =>
        {
            entity.HasKey(e => e.IDRol);

            entity.ToTable("Roluri");

            entity.Property(e => e.IDRol).HasColumnName("ID_Rol");
            entity.Property(e => e.Denumire).HasMaxLength(50);
        });

        modelBuilder.Entity<Sarcini>(entity =>
        {
            entity.HasKey(e => e.IDSarcina);

            entity.ToTable("Sarcini");

            entity.Property(e => e.IDSarcina).HasColumnName("ID_Sarcina");
            entity.Property(e => e.CalificativDePerformanta)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("Calificativ_de_Performanta");
            entity.Property(e => e.DataCreare).HasColumnName("Data_creare");
            entity.Property(e => e.DataDeFinalizat).HasColumnName("Data_de_finalizat");
            entity.Property(e => e.DataFinalizare).HasColumnName("Data_finalizare");
            entity.Property(e => e.Denumire).HasMaxLength(20);
            entity.Property(e => e.Descriere);
            entity.Property(e => e.IDFunctionalitate).HasColumnName("ID_Functionalitate");
            entity.Property(e => e.IDGradDificultate).HasColumnName("ID_Grad_Dificultate");
            entity.Property(e => e.IDGradUrgentaSarcina).HasColumnName("ID_Grad_Urgenta_Sarcina");
            entity.Property(e => e.MarcaAngajat).HasColumnName("Marca_Angajat");

            entity.HasOne(d => d.Functionalitate).WithMany(p => p.Sarcini)
                .HasForeignKey(d => d.IDFunctionalitate)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sarcini_Functionalitati");

            entity.HasOne(d => d.GradDeDificultate).WithMany(p => p.Sarcini)
                .HasForeignKey(d => d.IDGradDificultate)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sarcini_Grade_de_dificultate");

            entity.HasOne(d => d.GradUrgentaSarcina).WithMany(p => p.Sarcini);

            entity.HasOne(d => d.Angajat).WithMany(p => p.Sarcini)
                .HasForeignKey(d => d.MarcaAngajat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sarcini_Angajati");
        });

        modelBuilder.Entity<Subtask>(entity =>
        {
            entity.HasKey(e => e.IDSubtask);

            entity.Property(e => e.IDSubtask).HasColumnName("ID_Subtask");
            entity.Property(e => e.Denumire).HasMaxLength(20);
            entity.Property(e => e.IDSarcina).HasColumnName("ID_Sarcina");

            entity.HasOne(d => d.Sarcina).WithMany(p => p.Subtasks)
                .HasForeignKey(d => d.IDSarcina)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Subtasks_Sarcini");
        });

        modelBuilder.Entity<TipuriRapoarte>(entity =>
        {
            entity.HasKey(e => e.IDTipRaport);

            entity.ToTable("Tipuri_rapoarte");

            entity.Property(e => e.IDTipRaport).HasColumnName("ID_Tip_Raport");
            entity.Property(e => e.Denumire).HasMaxLength(100);
            entity.Property(e => e.Descriere);
        });

        modelBuilder.Entity<TipuriDocumenteInterne>(entity =>
        {
            entity.HasKey(e => e.IDTipDocument);

            entity.ToTable("Tipuri_Documente_Interne");

            entity.Property(e => e.IDTipDocument).HasColumnName("ID_Tip_Document");
            entity.Property(e => e.Denumire).HasMaxLength(100);
            entity.Property(e => e.Descriere);

        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}