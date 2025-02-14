
namespace NovaPropostaAnaliseCredito.Models
{
    public class ConfiguraPropostaResponse
    {
        public bool Sucesso { get; set; }
        public string MensagemTipo { get; set; }
        public string Mensagem { get; set; }
        public int CodEstabelecimento { get; set; }
        public ValidarDto Validar { get; set; }
        public ColetarDto Coletar { get; set; }
        public class ValidarDto
        {
            public bool Promotor { get; set; }
            public bool Celular { get; set; }
            public bool Email { get; set; }
        }
        public class ColetarDto
        {
            public Fase1Dto Fase1 { get; set; }
            public Fase2Dto Fase2 { get; set; }
            public string Biometria { get; set; }
        }
        public class Fase1Dto
        {
            public bool Promotor { get; set; }
            public bool Estabelecimento { get; set; }
            public bool CPF { get; set; }
            public bool Nascimento { get; set; }
            public bool Celular { get; set; }
            public bool Endereco { get; set; }
            public bool Nome { get; set; }
            public bool Mae { get; set; }
        }

        public class Fase2Dto
        {
            public bool RG { get; set; }
            public bool Renda { get; set; }
            public bool Entrega { get; set; }
        }



    }
}
