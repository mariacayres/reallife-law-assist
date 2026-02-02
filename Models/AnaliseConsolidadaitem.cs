using System;
using System.Collections.Generic;

namespace RealLifeLawAssist.Models
{
    public class AnaliseConsolidadaItem
    {
        public string? Arquivo { get; set; }
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        public int ScoreRisco { get; set; }
        public List<Risco> Riscos { get; set; } = new List<Risco>();
    }
}
