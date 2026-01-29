using System.Collections.Generic;

namespace RealLifeLawAssist.Models
{
    public class ClausulaFixa
    {
        public string Area { get; set; }
        public string Clausula { get; set; }
        public string Requisito { get; set; }
    }

    public class AspetoVariavel
    {
        public string Titulo { get; set; }
        public string Descricao { get; set; }
    }

    public class Penalidade
    {
        public string Nivel { get; set; }
        public string Exemplos { get; set; }
        public string Coima { get; set; }
        public string Compulsoria { get; set; }
    }

    public class Risco
    {
        public string Tipo { get; set; } // Alto / Baixo
        public string Titulo { get; set; }
        public string Descricao { get; set; }
    }

    public class AnaliseDados
    {
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string Objeto { get; set; }
        public string Localizacao { get; set; }
        public string TotalLinhas { get; set; }
        public decimal PrecoBase { get; set; }
        public decimal CustoKm { get; set; }
        public decimal KmMax { get; set; }
        public string Vigencia { get; set; }
        public string Caucao { get; set; }
        public string Pagamento { get; set; }
        public string Conclusao { get; set; }

        public List<ClausulaFixa> ClausulasFixas { get; set; } = new();
        public List<AspetoVariavel> AspetosVariaveis { get; set; } = new();
        public List<Penalidade> Penalidades { get; set; } = new();
        public List<Risco> Riscos { get; set; } = new();
    }
}
