namespace NovaPropostaAnaliseCredito.Models
{
    public class ConfirmaPropostaResponse
    {
        public bool Sucesso { get; set; }
        public string MensagemTipo { get; set; }
        public string Mensagem { get; set; }
        public string CodigoValidador { get; set; }
    }
}
