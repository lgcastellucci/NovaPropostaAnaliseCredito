namespace NovaPropostaAnaliseCredito.Models
{
    public class LojasProximasResponse
    {
        public bool Sucesso { get; set; }
        public string MensagemTipo { get; set; }
        public string Mensagem { get; set; }
        public List<EstabelecimentoDto> Estabelecimentos { get; set; }

        public class EstabelecimentoDto
        {
            public int CodEstabelecimento { get; set; }
            public string Descricao { get; set; }
            public double DistanciaKM { get; set; }
            public EnderecoDto Endereco { get; set; }
        }

        public class EnderecoDto
        {
            public string Cep { get; set; }
            public string Logradouro { get; set; }
            public string Numero { get; set; }
            public string Complemento { get; set; }
            public string Bairro { get; set; }
            public string Municipio { get; set; }
            public string UF { get; set; }
        }
    }
}
