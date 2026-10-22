using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusFlow.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        // NOCASE + índice único: "Admin" y "admin" son el mismo usuario (misma decisión que en Estudiantes).
        builder.Property(u => u.NombreUsuario)
            .IsRequired()
            .HasMaxLength(Usuario.MaxNombreUsuario)
            .UseCollation("NOCASE");

        builder.HasIndex(u => u.NombreUsuario).IsUnique();

        builder.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(Usuario.MaxNombreCompleto);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(Usuario.MaxPasswordHash);

        // Se guarda el nombre del rol ("Docente"), no el número: legible y a prueba de reordenar el enum.
        builder.Property(u => u.Rol).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.Property(u => u.Activo).IsRequired();
        builder.Property(u => u.DebeCambiarPassword).IsRequired();
        builder.Property(u => u.IntentosFallidos).IsRequired();
        builder.Property(u => u.FechaCreacion).IsRequired();
        // BloqueadoHasta y UltimoAcceso son nulables: sin configuración extra.
    }
}