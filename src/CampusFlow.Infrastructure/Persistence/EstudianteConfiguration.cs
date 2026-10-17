using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusFlow.Infrastructure.Persistence.Configurations;

public sealed class EstudianteConfiguration : IEntityTypeConfiguration<Estudiante>
{
    public void Configure(EntityTypeBuilder<Estudiante> builder)
    {
        builder.ToTable("Estudiantes");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Identificacion)
            .IsRequired()
            .HasMaxLength(Estudiante.MaxIdentificacion)
            .UseCollation("NOCASE");

        builder.HasIndex(e => e.Identificacion).IsUnique();

        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(Estudiante.MaxNombre);
        builder.Property(e => e.Apellidos).IsRequired().HasMaxLength(Estudiante.MaxApellidos);
        builder.Property(e => e.Correo).IsRequired().HasMaxLength(Estudiante.MaxCorreo);

        builder.Property(e => e.FechaIngreso).IsRequired();

        // Se guarda "Activo"/"Inactivo", no 1/2: legible en la BD y a prueba de reordenar el enum.
        builder.Property(e => e.Estado).IsRequired().HasConversion<string>().HasMaxLength(20);

        // Propiedad calculada: no es columna.
        builder.Ignore(e => e.NombreCompleto);
    }
}