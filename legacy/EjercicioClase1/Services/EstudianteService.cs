using System;
using System.Collections.Generic;
using System.Text;

namespace AplicaciónEscritorioEjercicio1.Services
{

    public class EstudianteService
    {
        public bool Validar(
            Estudiante? estudiante,
            out string error)
        {
            if (estudiante is null)
            {
                error = "No se recibió un estudiante.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                estudiante.NombreCompleto))
            {
                error = "El nombre completo es obligatorio.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
