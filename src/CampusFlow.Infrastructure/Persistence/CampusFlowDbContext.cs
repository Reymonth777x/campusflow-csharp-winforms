using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusFlow.Infrastructure.Persistence;

public sealed class CampusFlowDbContext(DbContextOptions<CampusFlowDbContext> options)
    : DbContext(options)
{
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Descubre todas las IEntityTypeConfiguration<T> del ensamblado:
        // cada entidad nueva se configura en su archivo, sin tocar este.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CampusFlowDbContext).Assembly);
    }
}