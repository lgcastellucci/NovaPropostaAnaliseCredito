namespace NovaPropostaAnaliseCredito.Models
{
    public class PesquisaPromotorResponse
    {
        public bool Sucesso { get; set; }
        public string MensagemTipo { get; set; }
        public string Mensagem { get; set; }
        public int Proposta { get; set; }
        public string Nome { get; set; }
        public List<int> VencimentosDia { get; set; }
        public List<LocalEntrega> LocaisEntrega { get; set; }

        public class Endereco
        {
            public string Cep { get; set; }
            public string Logradouro { get; set; }
            public string Numero { get; set; }
            public string Complemento { get; set; }
            public string Bairro { get; set; }
            public string Municipio { get; set; }
            public string UF { get; set; }
        }

        public class LocalEntrega
        {
            public int CodEstabelecimento { get; set; }
            public string Descricao { get; set; }
            public double DistanciaKM { get; set; }
            public Endereco Endereco { get; set; }
        }
    }
}
