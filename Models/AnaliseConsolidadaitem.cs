using System.Collections.Generic;

namespace RealLifeLawAssist.Models
{
    public class AnaliseConsolidadaItem
    {
        public string Arquivo { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Descricao { get; set; } = "";
        public int ScoreRisco { get; set; }
        public List<RiscoItem> Riscos { get; set; } = new();
    }

    public class RiscoItem
    {
        public string Tipo { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Descricao { get; set; } = "";
    }
}