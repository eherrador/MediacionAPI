using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Geth;
using Nethereum.JsonRpc.Client;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Hex.HexConvertors.Extensions;

namespace mediacionAPI.Controllers
{
    [Route("api/[controller]")]
    public class MediacionController : Controller
    {
        string mediadorAddress   = "0xfbedef61881d06f16b06c720280392f7c9e4dc05";  //clique
        //string mediadorAddress = "0x369d6Ecb1E319877dc5Dc8633Fc6EfA453423858";   //kaleido
        string cjaAddress        = "0xbee40e8faacd42298ddee9e4b93be85619812bea"; 
        string password = "Kiki32";   //clique
        //string password = "xt7u8OJu_88nAEvOeZ1wHqyT3Xz1a2cBuw0RRWQ3mL0";   //kaleido

        string abi = @"[{'constant':false,'inputs':[{'name':'descripcion','type':'bytes32'},{'name':'idMediacion','type':'uint32'},{'name':'ipfsHash','type':'bytes32'},{'name':'oficinaCJA','type':'address'}],'name':'creaNuevaMediacion','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'},{'name':'','type':'uint256'}],'name':'mediaciones','outputs':[{'name':'mediacionId','type':'uint32'},{'name':'terminada','type':'bool'},{'name':'nota','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'cja','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'mediador','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'idMediacion','type':'uint32'}],'name':'agregaInvitado','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'descripcion','type':'bytes32'},{'name':'idMediacion','type':'uint32'},{'name':'ipfsHash','type':'bytes32'}],'name':'agregaDocumento','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'anonymous':false,'inputs':[{'indexed':true,'name':'mediador','type':'address'},{'indexed':true,'name':'idMediacion','type':'uint32'},{'indexed':true,'name':'tipoDocto','type':'bytes32'},{'indexed':false,'name':'participante','type':'bytes32'}],'name':'SeCreoNuevaMediacion','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'mediador','type':'address'},{'indexed':false,'name':'idMediacion','type':'uint256'},{'indexed':false,'name':'tipoDocto','type':'bytes32'}],'name':'SeCreoNuevoDocumento','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'mediador','type':'address'},{'indexed':false,'name':'idMediacion','type':'uint256'},{'indexed':false,'name':'participante','type':'bytes32'}],'name':'SeCreoNuevoParticipante','type':'event'}]";
        string bytecode = "608060405234801561001057600080fd5b50610ba4806100206000396000f3006080604052600436106100775763ffffffff7c0100000000000000000000000000000000000000000000000000000000600035041663221f769c811461007c57806350394ac2146100c057806360242aa11461017a578063a350f151146101ab578063dac1d7a7146101c0578063f27834d9146101e0575b600080fd5b34801561008857600080fd5b506100ac60043563ffffffff60243516604435600160a060020a0360643516610204565b604080519115158252519081900360200190f35b3480156100cc57600080fd5b506100e4600160a060020a0360043516602435610520565b604051808463ffffffff1663ffffffff1681526020018315151515815260200180602001828103825283818151815260200191508051906020019080838360005b8381101561013d578181015183820152602001610125565b50505050905090810190601f16801561016a5780820380516001836020036101000a031916815260200191505b5094505050505060405180910390f35b34801561018657600080fd5b5061018f6105f8565b60408051600160a060020a039092168252519081900360200190f35b3480156101b757600080fd5b5061018f610607565b3480156101cc57600080fd5b506101de63ffffffff60043516610616565b005b3480156101ec57600080fd5b506101de60043563ffffffff602435166044356107bd565b600061020e61091e565b61021661091e565b60008054600160a060020a0333811673ffffffffffffffffffffffffffffffffffffffff19928316178355600180549188169190921617905560408051808201825289815260208101889052815180830190925293509081908152602001600090526002805463ffffffff191663ffffffff89161781556003805460018181018355600092835286517fc2575a0e9e593c00f959f8c92f12db2869c3395a3b0502d05e2516446f71f85b929094029182019390935560208601517fc2575a0e9e593c00f959f8c92f12db2869c3395a3b0502d05e2516446f71f85c90910155600480548084018083559190925283517f8a35acfbc15ff81a39ae7d344fd709f28e8600b4aa8c65c6b64bfe7fe36bd19b909201805494955090938593919291839160ff191690838181111561034757fe5b021790555060208201518154829061ff00191661010083600281111561036957fe5b0217905550506005805460ff1916905550506040805160608101825260298082527f45737461206573206c61206e6f74612061736f63696164612061206c61206d65602083019081527f6469616369c3b36e2e000000000000000000000000000000000000000000000092909301919091526103e791600691610935565b5060008054600160a060020a0316815260076020908152604082208054600181810180845592855292909320600280546005909502909101805463ffffffff191663ffffffff9095169490941784556003805492949193919261044d92840191906109b3565b50600282810180546104629284019190610a0f565b50600382810154908201805460ff191660ff9092161515919091179055600480830180546104a6928401919060026101006001831615026000190190911604610aa7565b5050604080517f536f6c69636974616e7465000000000000000000000000000000000000000000815290518a935063ffffffff8a169250600160a060020a033316917f776051d613d6412a12cc755c365a92dbec9c6e392ee7caf8de1b3143501a35e7919081900360200190a45060019695505050505050565b60076020528160005260406000208181548110151561053b57fe5b60009182526020918290206005909102018054600382015460048301805460408051601f6002600019600186161561010002019094169390930492830188900488028101880190915281815263ffffffff909416975060ff909216955092939192918301828280156105ee5780601f106105c3576101008083540402835291602001916105ee565b820191906000526020600020905b8154815290600101906020018083116105d157829003601f168201915b5050505050905083565b600154600160a060020a031681565b600054600160a060020a031681565b61061e61091e565b60005433600160a060020a039081169116141561069c57604080517f08c379a000000000000000000000000000000000000000000000000000000000815260206004820152601560248201527f4f706572616369c3b36e20696e76c3a16c6964612e0000000000000000000000604482015290519081900360640190fd5b50604080518082018252600181526000602080830182905233600160a060020a031682526007905291909120805463ffffffff84169081106106da57fe5b6000918252602080832060026005909302019190910180546001818101808455928552929093208451930180549193859391929091839160ff191690838181111561072157fe5b021790555060208201518154829061ff00191661010083600281111561074357fe5b02179055505060408051600160a060020a033316815263ffffffff861660208201527f496e76697461646f0000000000000000000000000000000000000000000000008183015290517f4ffaacc845fb2b428a42a513b6cd21dcf770f9a0a8e697f7351ede2c4b39dae69350908190036060019150a15050565b60005433600160a060020a0390811691161415806107ea575060015433600160a060020a03908116911614155b151561085757604080517f08c379a000000000000000000000000000000000000000000000000000000000815260206004820152601560248201527f4f706572616369c3b36e20696e76c3a16c6964612e0000000000000000000000604482015290519081900360640190fd5b600160a060020a0333166000908152600760205260409020805463ffffffff841690811061088157fe5b6000918252602080832060408051808201825288815280840187815260016005909602909301850180548087018255908752958490209051600290960201948555905193909201929092558051600160a060020a033316815263ffffffff851692810192909252818101859052517fdaca70434c366c31fb70c649895c3db78ef439b116ab21ba622743a459daa32a9181900360600190a1505050565b604080518082019091526000808252602082015290565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f1061097657805160ff19168380011785556109a3565b828001600101855582156109a3579182015b828111156109a3578251825591602001919060010190610988565b506109af929150610b1c565b5090565b828054828255906000526020600020906002028101928215610a035760005260206000209160020282015b82811115610a03578254825560018084015490830155600292830192909101906109de565b506109af929150610b39565b828054828255906000526020600020908101928215610a9b5760005260206000209182015b82811115610a9b57825482548491849160ff90911690829060ff191660018381811115610a5d57fe5b02179055508154815460ff610100928390041691839161ff00191690836002811115610a8557fe5b0217905550505091600101919060010190610a34565b506109af929150610b59565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f10610ae057805485556109a3565b828001600101855582156109a357600052602060002091601f016020900482015b828111156109a3578254825591600101919060010190610b01565b610b3691905b808211156109af5760008155600101610b22565b90565b610b3691905b808211156109af5760008082556001820155600201610b3f565b610b3691905b808211156109af57805461ffff19168155600101610b5f5600a165627a7a723058205c9792fc6f1c2cf03dca1df8eba8382a060b3e9fadb50e41d01892a3f6b23bd20029";

        //Web3 geth;  //Kaleido
        Web3Geth geth;   //Clique

        ManagedAccount account;
        
        private SmartContractConfiguration _smartContractConfiguration;

        public MediacionController(IOptions<SmartContractConfiguration> SmartContractConfigurationAccessor)
        {
            _smartContractConfiguration = SmartContractConfigurationAccessor.Value;
        }

        //Obtenen Token de Mediación
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        //Crea Token de Mediación
        [HttpPost]
        //public async IEnumerable<string> Post()
        public async Task<IActionResult> Post(int idMediacion, [FromBody]JArray documentos)
        {
            try 
            {
                CreaAccountyGeth();

                await VerificaExistenciaSmartContract();
                
                var contract = geth.Eth.GetContract(abi, _smartContractConfiguration.SmartContractAddress);

                var creaNuevaMediacionFunction = contract.GetFunction("creaNuevaMediacion");
                var mediacionStruct = contract.GetFunction("mediaciones");
                var seCreoNuevaMediacionEvent = contract.GetEvent("SeCreoNuevaMediacion");

                foreach (JObject item in documentos)
                {
                    string descripcion = item.GetValue("descripcion").ToString();
                    string ipfsHash = item.GetValue("ipfsHash").ToString();

                    cjaAddress = cjaAddress.ToLower().RemoveHexPrefix().PadLeft(40,'0').EnsureHexPrefix();
                    mediadorAddress = mediadorAddress.ToLower().RemoveHexPrefix().PadLeft(40,'0').EnsureHexPrefix();

                    //var filterEvents = await seCreoNuevaMediacionEvent.CreateFilterAsync();

                    var transactionHash = await creaNuevaMediacionFunction.SendTransactionAsync(mediadorAddress, new HexBigInteger(900000), null, descripcion, Convert.ToInt32(idMediacion), ipfsHash, cjaAddress);

                    var receipt = await geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                    while (receipt == null){
                        Thread.Sleep(5000);
                        receipt = await geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                    }

                    //var logEvents = await seCreoNuevaMediacionEvent.GetFilterChanges<SeCreoNuevaMediacionEvent>(filterEvents);
                }
            }
            catch (Exception chingadamadre)
            {
                Console.WriteLine("Message: " + chingadamadre.Message);
                Console.WriteLine("InnerException: " + chingadamadre.InnerException);
                Console.WriteLine("Source: " + chingadamadre.Source);
                return BadRequest(error: chingadamadre.Message);  //400
            }

            return Ok("Se ha creado una nueva Mediacion con id = " + idMediacion.ToString());
        }

        //Actualiza Token de Mediciación
        [HttpPut]
        public async Task<IActionResult> Put(int idMediacion, [FromBody]JArray documentos)
        {
            try
            {
                CreaAccountyGeth();

                await VerificaExistenciaSmartContract();
                
                var contract = geth.Eth.GetContract(abi, _smartContractConfiguration.SmartContractAddress);

                var agregaDocumentoFunction = contract.GetFunction("agregaDocumento");
                var mediacionStruct = contract.GetFunction("mediaciones");
                var seCreoNuevaDocumentoEvent = contract.GetEvent("SeCreoNuevoDocumento");

                foreach (JObject item in documentos)
                {
                    string descripcion = item.GetValue("descripcion").ToString();
                    string ipfsHash = item.GetValue("ipfsHash").ToString();

                    mediadorAddress = mediadorAddress.ToLower().RemoveHexPrefix().PadLeft(40,'0').EnsureHexPrefix();

                    var transactionHash = await agregaDocumentoFunction.SendTransactionAsync(mediadorAddress, new HexBigInteger(900000), null, descripcion, Convert.ToInt32(idMediacion), ipfsHash);

                    var receipt = await geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                    while (receipt == null){
                        Thread.Sleep(5000);
                        receipt = await geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                    }
                } 
            }
            catch (Exception chingadamadre)
            {
                Console.WriteLine("Message: " + chingadamadre.Message);
                Console.WriteLine("InnerException: " + chingadamadre.InnerException);
                Console.WriteLine("Source: " + chingadamadre.Source);
                return BadRequest(error: chingadamadre.Message);  //400
            }

            //return new string[] { "value1", "value2" };
            return Ok("Se han agregados nuevos documentos a la Mediacion " + idMediacion.ToString());
        }

        private void CreaAccountyGeth()
        {
            account = new ManagedAccount(mediadorAddress, password);  //Kaleido + Clique
            geth = new Web3Geth(account, "http://localhost:8501");  //Clique
            /**** Kaleido ****/
            /*var byteArray = Encoding.ASCII.GetBytes("u0n94z8hht:xt7u8OJu_88nAEvOeZ1wHqyT3Xz1a2cBuw0RRWQ3mL0");
            AuthenticationHeaderValue autenticacion = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            var client = new RpcClient(new Uri("https://u0yaaa1f6n-u0s4p500nd-rpc.us-east-2.kaleido.io"),autenticacion);
            var geth = new Web3(client);*/
            /*****************/
        }

        private async Task VerificaExistenciaSmartContract() 
        {
            //if (contractAddress == string.Empty)
            if (_smartContractConfiguration.SmartContractDeployed == false)
            {
                try
                {
                    mediadorAddress = mediadorAddress.ToLower().RemoveHexPrefix().PadLeft(40,'0').EnsureHexPrefix();

                    var transactionHash = await geth.Eth.DeployContract.SendRequestAsync(abi, bytecode, mediadorAddress, new HexBigInteger(900000));
                    var receipt = await geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                    while (receipt == null)
                    {
                        Thread.Sleep(5000);
                        receipt = await geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                    }

                    _smartContractConfiguration.SmartContractDeployed = true;
                    _smartContractConfiguration.SmartContractAddress = receipt.ContractAddress;
                }
                catch (Exception chingadamadre)
                {
                    Console.WriteLine("Message: " + chingadamadre.Message);
                    Console.WriteLine("InnerException: " + chingadamadre.InnerException);
                    Console.WriteLine("Source: " + chingadamadre.Source);
                    throw chingadamadre;
                }
            }
        }
    }

    [FunctionOutput]
    public class SeCreoNuevaMediacionEvent
    {
        [Parameter("address", "mediador", 1, true)]
        public string Mediador {get; set;}

        [Parameter("uint32", "idMediacion", 2, true)]
        public BigInteger IdMediacion {get; set;}

        [Parameter("string", "tipoDocto", 3, true)]
        public string TipoDocto {get; set;}

        [Parameter("string", "participante", 4, false)]
        public string Participante {get; set;}
    }
}