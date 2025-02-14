namespace NovaPropostaAnaliseCredito.Models
{
    public class ProposalModel
    {
        /* campos que serão preenchidos na requisição da API */
        public int? numeroProposta { get; set; }
        public string? cpfPromotor { get; set; }
        public int? codEstabelecimentoProximo { get; set; }
        public string? cpf { get; set; }
        public DateTime? birthDate { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public int? vencimentosDia { get; set; }
        public int? codEstabelecimentoEntrega { get; set; }
        public string? codigoValidador { get; set; }
        public string? assertivaPedidoID { get; set; }


        /* campos que serão preenchidos na resposta da API */
        public ConfiguraPropostaResponse? configuraPropostaResponse;
        public LojasProximasResponse? lojasProximasResponse;
        public IniciaPropostaResponse? iniciaPropostaResponse;
        public ConfirmaPropostaResponse? confirmaPropostaResponse;
    }

    public class ProposalModelJson {
        public string? conteudo { get; set; }
    }
}
