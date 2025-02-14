using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NovaPropostaAnaliseCredito.Models;
using NovaPropostaAnaliseCredito.Services;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace NovaPropostaAnaliseCredito.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult Index()
        {

            #region Fazer a chamada para a API para checar nas configurações se precisamos Coletar CPF do Promotor
            var requisicao = new
            {
                CodOperadora = GlobalsInfo.codOperadora,
                ChaveIntegrador = GlobalsInfo.chaveIntegrador
            };

            var httpService = new HttpService("Index");
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.UrlSet(GlobalsInfo.endPoint + "/propostas/configura");
            httpService.PayLoadSet(JsonConvert.SerializeObject(requisicao), Encoding.UTF8, "application/json");
            var retHttp = httpService.ExecutePost();

            if (retHttp.erro)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (retHttp.httpStatusCode != System.Net.HttpStatusCode.OK)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (string.IsNullOrWhiteSpace(retHttp.responseBody))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            #endregion

            var configuraPropostaResponse = JsonConvert.DeserializeObject<ConfiguraPropostaResponse>(retHttp.responseBody);
            if (configuraPropostaResponse == null)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            var proposal = new ProposalModel();
            proposal.configuraPropostaResponse = configuraPropostaResponse;

            if (configuraPropostaResponse.Coletar.Fase1.Promotor)
            {
                TempData["ProposalJson"] = JsonConvert.SerializeObject(proposal);
                return RedirectToAction("ColetarCpfPromotor");
            }

            return View();
        }

        public IActionResult ColetarCpfPromotor()
        {
            var proposal = new ProposalModel();
            if (TempData["ProposalJson"] != null)
            {
                ViewBag.ProposalJson = TempData["ProposalJson"].ToString();
                proposal = JsonConvert.DeserializeObject<ProposalModel>(TempData["ProposalJson"].ToString());
            }

            ///SOMENTE PARA FACILITAR O TESTE
            proposal.cpfPromotor = "111.222.333-96";

            return View(proposal);
        }

        public IActionResult SubmitCpfPromotor(ProposalModel proposal, string ProposalJson)
        {
            if (proposal == null || string.IsNullOrWhiteSpace(proposal.cpfPromotor))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            if (!string.IsNullOrWhiteSpace(ProposalJson))
            {
                var proposalFromJson = JsonConvert.DeserializeObject<ProposalModel>(ProposalJson);
                if (proposalFromJson != null)
                {
                    proposalFromJson.cpfPromotor = proposal.cpfPromotor;
                    proposal = proposalFromJson;
                }
            }

            // Processar os dados do formulário aqui
            _logger.LogInformation("CPF do promotor recebido: {@Proposal.cpfPromotor}", proposal.cpfPromotor);

            var requisicao = new
            {
                CodOperadora = GlobalsInfo.codOperadora,
                ChaveIntegrador = GlobalsInfo.chaveIntegrador,
                CPF = proposal.cpfPromotor
            };

            var httpService = new HttpService("SubmitCpfPromotor");
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.UrlSet(GlobalsInfo.endPoint + "/propostas/pesquisa/promotor");
            httpService.PayLoadSet(JsonConvert.SerializeObject(requisicao), Encoding.UTF8, "application/json");
            var retHttp = httpService.ExecutePost();

            if (retHttp.erro)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (retHttp.httpStatusCode != System.Net.HttpStatusCode.OK)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (string.IsNullOrWhiteSpace(retHttp.responseBody))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            var response = JsonConvert.DeserializeObject<dynamic>(retHttp.responseBody);
            if (response == null || response.Sucesso != true)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            _logger.LogInformation("Promotor processado");

            if (proposal.configuraPropostaResponse.Coletar.Fase1.Estabelecimento)
            {
                TempData["ProposalJson"] = JsonConvert.SerializeObject(proposal);
                return RedirectToAction("ColetarLojasProximas");
            }

            return View();
        }

        public IActionResult ColetarLojasProximas()
        {
            var proposal = new ProposalModel();
            if (TempData["ProposalJson"] != null)
            {
                ViewBag.ProposalJson = TempData["ProposalJson"].ToString();
                proposal = JsonConvert.DeserializeObject<ProposalModel>(TempData["ProposalJson"].ToString());
            }

            var requisicao = new
            {
                CodOperadora = GlobalsInfo.codOperadora,
                ChaveIntegrador = GlobalsInfo.chaveIntegrador,
                Latitude = "-22.7256039",
                Longitude = "-47.6487915"
            };

            var httpService = new HttpService("SubmitLojasProximas");
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.UrlSet(GlobalsInfo.endPoint + "/propostas/pesquisa/estabelecimentos");
            httpService.PayLoadSet(JsonConvert.SerializeObject(requisicao), Encoding.UTF8, "application/json");
            var retHttp = httpService.ExecutePost();

            if (retHttp.erro)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (retHttp.httpStatusCode != System.Net.HttpStatusCode.OK)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (string.IsNullOrWhiteSpace(retHttp.responseBody))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            var response = JsonConvert.DeserializeObject<dynamic>(retHttp.responseBody);
            if (response == null || response.Sucesso != true)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            var lojasProximasResponse = JsonConvert.DeserializeObject<LojasProximasResponse>(retHttp.responseBody);
            if (lojasProximasResponse == null)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            proposal.lojasProximasResponse = lojasProximasResponse;
            ViewBag.ProposalJson = JsonConvert.SerializeObject(proposal);
            return View(proposal);
        }

        public IActionResult SubmitLojasProximas(ProposalModel proposal, string ProposalJson)
        {
            if (proposal == null || proposal.codEstabelecimentoProximo == null)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            if (!string.IsNullOrWhiteSpace(ProposalJson))
            {
                var proposalFromJson = JsonConvert.DeserializeObject<ProposalModel>(ProposalJson);
                if (proposalFromJson != null)
                {
                    proposalFromJson.codEstabelecimentoProximo = proposal.codEstabelecimentoProximo;
                    proposal = proposalFromJson;
                }
            }

            TempData["ProposalJson"] = JsonConvert.SerializeObject(proposal);
            return RedirectToAction("ColetarProposta");
        }

        public IActionResult ColetarProposta()
        {
            var proposal = new ProposalModel();
            if (TempData["ProposalJson"] != null)
                ViewBag.ProposalJson = TempData["ProposalJson"].ToString();

            ///SOMENTE PARA FACILITAR O TESTE
            proposal.cpf = "111.222.333-96";
            proposal.birthDate = Convert.ToDateTime("01/01/1980");
            proposal.email = "teste@google.net";
            proposal.phone = "1999990000";

            return View(proposal);
        }

        public IActionResult SubmitProposta(ProposalModel proposal, string ProposalJson)
        {
            if (proposal == null || string.IsNullOrWhiteSpace(proposal.cpf))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            if (!string.IsNullOrWhiteSpace(ProposalJson))
            {
                var proposalFromJson = JsonConvert.DeserializeObject<ProposalModel>(ProposalJson);
                if (proposalFromJson != null)
                {
                    proposalFromJson.cpf = proposal.cpf;
                    proposalFromJson.birthDate = proposal.birthDate;
                    proposalFromJson.phone = proposal.phone;
                    proposalFromJson.email = proposal.email;
                    proposal = proposalFromJson;
                }
            }

            // Processar os dados do formulário aqui
            _logger.LogInformation("Proposta recebida CPF: {@Proposal.CPF}", proposal.cpf);

            var requisicao = new
            {
                CodOperadora = GlobalsInfo.codOperadora,
                ChaveIntegrador = GlobalsInfo.chaveIntegrador,
                CodEstabelecimento = 3033,
                CPF = proposal.cpf,
                Nascimento = proposal.birthDate,
                Celular = proposal.phone,
                Email = proposal.email,
                PromotorCPF = "string",
                Endereco = new
                {
                    Cep = "13417530",
                    Logradouro = "string",
                    Numero = 1,
                    Complemento = "string",
                    Bairro = "string",
                    Municipio = "string",
                    UF = "string"
                }
            };

            var httpService = new HttpService("SubmitProposal");
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.UrlSet(GlobalsInfo.endPoint + "/propostas/inicia");
            httpService.PayLoadSet(JsonConvert.SerializeObject(requisicao), Encoding.UTF8, "application/json");
            var retHttp = httpService.ExecutePost();

            if (retHttp.erro)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (retHttp.httpStatusCode != System.Net.HttpStatusCode.OK)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (string.IsNullOrWhiteSpace(retHttp.responseBody))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            var iniciaPropostaResponse = JsonConvert.DeserializeObject<IniciaPropostaResponse>(retHttp.responseBody);
            if (iniciaPropostaResponse == null)
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            if (!iniciaPropostaResponse.Sucesso)
            {
                TempData["ErrorMessage"] = iniciaPropostaResponse.Mensagem;
                return RedirectToAction("ColetarProposta");
            }

            proposal.iniciaPropostaResponse = iniciaPropostaResponse;
            proposal.numeroProposta = iniciaPropostaResponse.Proposta;

            _logger.LogInformation("Proposta criada: " + proposal.iniciaPropostaResponse.Proposta);

            TempData["ProposalJson"] = JsonConvert.SerializeObject(proposal);
            return RedirectToAction("ColetarVenctoEntrega");
        }

        public IActionResult ColetarVenctoEntrega()
        {
            var proposal = new ProposalModel();
            if (TempData["ProposalJson"] != null)
            {
                ViewBag.ProposalJson = TempData["ProposalJson"].ToString();
                proposal = JsonConvert.DeserializeObject<ProposalModel>(TempData["ProposalJson"].ToString());
            }

            return View(proposal);
        }

        public IActionResult SubmitVenctoEntrega(ProposalModel proposal, string ProposalJson)
        {
            if (proposal == null || proposal.vencimentosDia == null)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            if (!string.IsNullOrWhiteSpace(ProposalJson))
            {
                var proposalFromJson = JsonConvert.DeserializeObject<ProposalModel>(ProposalJson);
                if (proposalFromJson != null)
                {
                    proposalFromJson.codEstabelecimentoEntrega = proposal.codEstabelecimentoEntrega;
                    proposalFromJson.vencimentosDia = proposal.vencimentosDia;
                    proposal = proposalFromJson;
                }
            }

            // Processar os dados do formulário aqui
            _logger.LogInformation("Confirmando proposta: {@Proposal.numeroProposta}", proposal.numeroProposta);

            var requisicao = new
            {
                CodOperadora = GlobalsInfo.codOperadora,
                ChaveIntegrador = GlobalsInfo.chaveIntegrador,
                Proposta = proposal.numeroProposta,
                CPF = proposal.cpf,
                VencimentoDia = proposal.vencimentosDia,
                CodEstabelecimento = proposal.codEstabelecimentoEntrega
            };

            var httpService = new HttpService("SubmitProposal");
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.UrlSet(GlobalsInfo.endPoint + "/propostas/confirma");
            httpService.PayLoadSet(JsonConvert.SerializeObject(requisicao), Encoding.UTF8, "application/json");
            var retHttp = httpService.ExecutePost();

            if (retHttp.erro)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (retHttp.httpStatusCode != System.Net.HttpStatusCode.OK)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (string.IsNullOrWhiteSpace(retHttp.responseBody))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            var confirmaPropostaResponse = JsonConvert.DeserializeObject<ConfirmaPropostaResponse>(retHttp.responseBody);
            if (confirmaPropostaResponse == null)
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            if (!confirmaPropostaResponse.Sucesso)
            {
                TempData["ErrorMessage"] = confirmaPropostaResponse.Mensagem;
                return RedirectToAction("ColetarVenctoEntrega");
            }

            proposal.confirmaPropostaResponse = confirmaPropostaResponse;

            _logger.LogInformation("Proposta confirmada: " + proposal.numeroProposta);

            TempData["ProposalJson"] = JsonConvert.SerializeObject(proposal);
            return RedirectToAction("ColetarCodigoValidador");
        }

        public IActionResult SubmitAlteraCelular(ProposalModel proposal, string ProposalJson)
        {
            if (proposal == null || string.IsNullOrWhiteSpace(proposal.phone))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            string newPhone = proposal.phone;
            if (!string.IsNullOrWhiteSpace(ProposalJson))
            {
                var proposalFromJson = JsonConvert.DeserializeObject<ProposalModel>(ProposalJson);
                if (proposalFromJson != null)
                {
                    proposal = proposalFromJson;
                }
            }

            var requisicao = new
            {
                CodOperadora = GlobalsInfo.codOperadora,
                ChaveIntegrador = GlobalsInfo.chaveIntegrador,
                Proposta = proposal.numeroProposta,
                CPF = proposal.cpf,
                Celular = newPhone
            };

            var httpService = new HttpService("SubmitAlteraCelular");
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.UrlSet(GlobalsInfo.endPoint + "/propostas/altera");
            httpService.PayLoadSet(JsonConvert.SerializeObject(requisicao), Encoding.UTF8, "application/json");
            var retHttp = httpService.ExecutePost();

            if (retHttp.erro)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (retHttp.httpStatusCode != System.Net.HttpStatusCode.OK)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (string.IsNullOrWhiteSpace(retHttp.responseBody))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            JObject dataJson;
            try
            {
                dataJson = JObject.Parse(retHttp.responseBody);
            }
            catch
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            if (dataJson.SelectToken("Sucesso") == null)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            if (dataJson.SelectToken("Sucesso").ToString().ToUpper() == "TRUE")
                proposal.phone = newPhone;

            TempData["ProposalJson"] = JsonConvert.SerializeObject(proposal);

            if (dataJson.SelectToken("Sucesso").ToString().ToUpper() != "TRUE")
                TempData["ErrorMessage"] = dataJson.SelectToken("Mensagem").ToString();

            if (dataJson.SelectToken("Sucesso").ToString().ToUpper() != "TRUE")
            {
                //post /api/private/propostas/enviaAviso
                var requisicaoAviso = new
                {
                    CodOperadora = GlobalsInfo.codOperadora,
                    ChaveIntegrador = GlobalsInfo.chaveIntegrador,
                    Proposta = proposal.numeroProposta,
                    CPF = proposal.cpf,
                    Tipo = "C",
                    TipoDestino = "S"
                };

                var httpServiceAviso = new HttpService("SubmitAlteraCelular");
                httpServiceAviso.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
                httpServiceAviso.UrlSet(GlobalsInfo.endPoint + "/propostas/enviaAviso");
                httpServiceAviso.PayLoadSet(JsonConvert.SerializeObject(requisicaoAviso), Encoding.UTF8, "application/json");
                var retHttpAviso = httpServiceAviso.ExecutePost();

                if (retHttpAviso.erro)
                    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                if (retHttpAviso.httpStatusCode != System.Net.HttpStatusCode.OK)
                    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                if (string.IsNullOrWhiteSpace(retHttpAviso.responseBody))
                    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

                JObject dataJsonAviso;
                try
                {
                    dataJsonAviso = JObject.Parse(retHttpAviso.responseBody);
                }
                catch
                {
                    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                }

                if (dataJson.SelectToken("Sucesso").ToString().ToUpper() != "TRUE")
                {
                    TempData["ErrorMessage"] = dataJson.SelectToken("Mensagem").ToString();
                    return RedirectToAction("ColetarVenctoEntrega");
                }
            }

            return RedirectToAction("ColetarCodigoValidador");
        }

        public IActionResult ColetarCodigoValidador()
        {
            var proposal = new ProposalModel();
            if (TempData["ProposalJson"] != null)
            {
                ViewBag.ProposalJson = TempData["ProposalJson"].ToString();
                proposal = JsonConvert.DeserializeObject<ProposalModel>(TempData["ProposalJson"].ToString());
            }

            return View(proposal);
        }

        public IActionResult SubmitCodigoValidador(ProposalModel proposal, string ProposalJson)
        {
            if (proposal == null || string.IsNullOrWhiteSpace(proposal.codigoValidador))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            string newCodigoValidador = proposal.codigoValidador;
            if (!string.IsNullOrWhiteSpace(ProposalJson))
            {
                var proposalFromJson = JsonConvert.DeserializeObject<ProposalModel>(ProposalJson);
                if (proposalFromJson != null)
                {
                    proposal = proposalFromJson;
                }
            }
            proposal.codigoValidador = newCodigoValidador;

            var requisicao = new
            {
                CodOperadora = GlobalsInfo.codOperadora,
                ChaveIntegrador = GlobalsInfo.chaveIntegrador,
                Proposta = proposal.numeroProposta,
                CPF = proposal.cpf,
                CodigoValidador = proposal.codigoValidador
            };

            var httpService = new HttpService("SubmitCodigovalidador");
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.UrlSet(GlobalsInfo.endPoint + "/propostas/criaFluxo");
            httpService.PayLoadSet(JsonConvert.SerializeObject(requisicao), Encoding.UTF8, "application/json");
            var retHttp = httpService.ExecutePost();

            if (retHttp.erro)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (retHttp.httpStatusCode != System.Net.HttpStatusCode.OK)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (string.IsNullOrWhiteSpace(retHttp.responseBody))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            JObject dataJson;
            try
            {
                dataJson = JObject.Parse(retHttp.responseBody);
            }
            catch
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            if (dataJson.SelectToken("Sucesso").ToString().ToUpper() != "TRUE")
            {
                TempData["ErrorMessage"] = dataJson.SelectToken("Mensagem").ToString();
                return RedirectToAction("ColetarCodigoValidador");
            }

            TempData["ProposalJson"] = JsonConvert.SerializeObject(proposal);
            return RedirectToAction("PaginaAssertiva");
        }

        public IActionResult PaginaAssertiva()
        {
            var proposal = new ProposalModel();
            if (TempData["ProposalJson"] != null)
            {
                ViewBag.ProposalJson = TempData["ProposalJson"].ToString();
                proposal = JsonConvert.DeserializeObject<ProposalModel>(TempData["ProposalJson"].ToString());
            }

            return View(proposal);
        }

        public IActionResult SubmitPaginaAssertiva(ProposalModel proposal, string ProposalJson)
        {
            if (proposal == null || string.IsNullOrWhiteSpace(proposal.assertivaPedidoID))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            string newAssertivaPedidoID = proposal.assertivaPedidoID;
            if (!string.IsNullOrWhiteSpace(ProposalJson))
            {
                var proposalFromJson = JsonConvert.DeserializeObject<ProposalModel>(ProposalJson);
                if (proposalFromJson != null)
                {
                    proposal = proposalFromJson;
                }
            }
            proposal.assertivaPedidoID = newAssertivaPedidoID;

            var requisicao = new
            {
                dados = new
                {
                    entidade = "PEDIDO",
                    status = "FINALIZADO",
                    id = proposal.assertivaPedidoID
                }
            };

            var httpService = new HttpService("SubmitPaginaAssertiva");
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.UrlSet(GlobalsInfo.endPoint + "/propostas/finaliza/" + GlobalsInfo.codOperadora.ToString().PadLeft(3, '0') + "/assertiva");
            httpService.PayLoadSet(JsonConvert.SerializeObject(requisicao), Encoding.UTF8, "application/json");
            var retHttp = httpService.ExecutePost();

            if (retHttp.erro)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (retHttp.httpStatusCode != System.Net.HttpStatusCode.OK)
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            if (string.IsNullOrWhiteSpace(retHttp.responseBody))
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            JObject dataJson;
            try
            {
                dataJson = JObject.Parse(retHttp.responseBody);
            }
            catch
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            if (dataJson.SelectToken("Sucesso").ToString().ToUpper() != "TRUE")
            {
                TempData["ErrorMessage"] = dataJson.SelectToken("Mensagem").ToString();
                return RedirectToAction("PaginaAssertiva");
            }

            TempData["ProposalJson"] = JsonConvert.SerializeObject(proposal);
            return RedirectToAction("PaginaFinal");
        }

        public IActionResult PaginaFinal()
        {
            var proposal = new ProposalModel();
            if (TempData["ProposalJson"] != null)
            {
                ViewBag.ProposalJson = TempData["ProposalJson"].ToString();
                proposal = JsonConvert.DeserializeObject<ProposalModel>(TempData["ProposalJson"].ToString());
            }

            return View(proposal);
        }
    }
}
