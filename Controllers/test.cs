using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Geth;
using Nethereum.JsonRpc.Client;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3.Accounts.Managed;

namespace mediacionAPI.Controllers
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        string senderAddress = "0xfbedef61881d06f16b06c720280392f7c9e4dc05";
        string password = "Kiki32";
        string abi = @"[{'constant':false,'inputs':[{'name':'a','type':'int256'}],'name':'multiply','outputs':[{'name':'r','type':'int256'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'inputs':[{'name':'multiplier','type':'int256'}],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'a','type':'int256'},{'indexed':true,'name':'sender','type':'address'},{'indexed':false,'name':'result','type':'int256'}],'name':'Multiplied','type':'event'}]";
        string bytecode = "608060405234801561001057600080fd5b50604051602080610125833981016040525160005560f2806100336000396000f300608060405260043610603e5763ffffffff7c01000000000000000000000000000000000000000000000000000000006000350416631df4f14481146043575b600080fd5b348015604e57600080fd5b506058600435606a565b60408051918252519081900360200190f35b60008054820290503373ffffffffffffffffffffffffffffffffffffffff16827f841774c8b4d8511a3974d7040b5bc3c603d304c926ad25d168dacd04e25c4bed836040518082815260200191505060405180910390a39190505600a165627a7a7230582044e61e276464ebaf90046ddefbaa8b8065a2b088a5857b8d8a306411863b34cf0029";
        int multiplier = 7;
        Web3Geth geth;
        ManagedAccount account;
        string contractAddress = string.Empty;

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            await VerificaExistenciaSmartContract();
            var contract = geth.Eth.GetContract(abi, contractAddress);

            var multiplyFunction = contract.GetFunction("multiply");
            var multiplyEvent = contract.GetEvent("Multiplied");

            var filterAll = await multiplyEvent.CreateFilterAsync();
            var filter7 = await multiplyEvent.CreateFilterAsync(7);

            var transactionHash = await multiplyFunction.SendTransactionAsync(senderAddress, new HexBigInteger(900000), null, 7);

            var receipt = await geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            while (receipt == null){
                Thread.Sleep(5000);
                receipt = await geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }

            receipt = await multiplyFunction.SendTransactionAndWaitForReceiptAsync(senderAddress, new HexBigInteger(900000), null, null, 8);

            var log = await multiplyEvent.GetFilterChanges<MultipliedEvent>(filterAll);
            var log7 = await multiplyEvent.GetFilterChanges<MultipliedEvent>(filter7);

            return Ok(new string[] { "value1", "value2" });
        }

        [HttpPut]
        public IEnumerable<string> Put()
        {
            return new string[] { "value1", "value2" };
        }

        private async Task VerificaExistenciaSmartContract() 
        {
            if (contractAddress == string.Empty)
            {
                account = new ManagedAccount(senderAddress, password);
                geth = new Web3Geth(account, "http://localhost:8501");
                try
                {
                    var transactionHash = await geth.Eth.DeployContract.SendRequestAsync(abi, bytecode, senderAddress, new Nethereum.Hex.HexTypes.HexBigInteger(900000),5);
                    var receipt = await geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                    while (receipt == null)
                    {
                        Thread.Sleep(5000);
                        receipt = await geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                    }
                    contractAddress = receipt.ContractAddress;
                }
                catch (Exception chingadamadre)
                {
                    Console.WriteLine("Message: " + chingadamadre.Message);
                    Console.WriteLine("InnerException: " + chingadamadre.InnerException);
                    Console.WriteLine("Source: " + chingadamadre.Source);
                }
            }
        }
    }

    public class MultipliedEvent
    {
        [Parameter("int", "a", 1, true)]
        public int MultiplicationInput {get; set;}

        [Parameter("address", "sender", 2, true)]
        public string Sender {get; set;}

        [Parameter("int", "result", 3, false)]
        public int Result {get; set;}
        }
}