using System;
using System.Collections.Generic;
using System.Text;

namespace AplicaciónEscritorioEjercicio1
{
    public class Estudiante
    {
        public int Id { get; set; }

        public string Identificacion { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public DateTime FechaIngreso { get; set; } = DateTime.Today;
    }
}
