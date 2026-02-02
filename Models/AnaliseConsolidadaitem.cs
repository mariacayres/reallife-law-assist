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
        public List<string> Badges { get; set; } = new(); // badges associados ao item
        public string OutputHtmlPath { get; set; } = "";        // caminho para o HTML individual
    }

    public class RiscoItem
    {
        public string Tipo { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Descricao { get; set; } = ""; // se quiser exibir descrição detalhada
    }
}