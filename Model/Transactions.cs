using System.Collections.Generic;
using System.Security.Cryptography.Xml;

public class ContractCallDecodedData
{
    public string contractName { get; set; }
    public List<object> functionCalls { get; set; }
}

public class SourceDestination
{
    public string type { get; set; }
    public string id { get; set; }
    public string name { get; set; }
    public string subType { get; set; }
}

public class NetworkRecord
{
    public SourceDestination source { get; set; }
    public SourceDestination destination { get; set; }
    public string txHash { get; set; }
    public string networkFee { get; set; }
    public string assetId { get; set; }
    public string netAmount { get; set; }
    public bool isDropped { get; set; }
    public string type { get; set; }
    public string destinationAddress { get; set; }
    public string sourceAddress { get; set; }
    public string amountUSD { get; set; }
    public int index { get; set; }
    public RewardInfo rewardInfo { get; set; }
}

public class AmlScreeningResult
{
    public string provider { get; set; }
    public object payload { get; set; }
}

public class Destination
{
    public string amount { get; set; }
    public string amountUSD { get; set; }
    public AmlScreeningResult amlScreeningResult { get; set; }
    public SourceDestination destination { get; set; }
    public AuthorizationInfo authorizationInfo { get; set; }
}

public class AmountInfo
{
    public string amount { get; set; }
    public string requestedAmount { get; set; }
    public string netAmount { get; set; }
    public string amountUSD { get; set; }
}

public class FeeInfo
{
    public string networkFee { get; set; }
    public string serviceFee { get; set; }
    public string gasPrice { get; set; }
}

public class SignedMessages
{
    public string content { get; set; }
    public string algorithm { get; set; }
    public List<int> derivationPath { get; set; }
    public Signature signature { get; set; }
    public string publicKey { get; set; }
}

public class AuthorizationInfo
{
    public bool allowOperatorAsAuthorizer { get; set; }
    public string logic { get; set; }
    public List<Group> groups { get; set; }
}

public class BlockInfo
{
    public string blockHeight { get; set; }
    public string blockHash { get; set; }
}

public class RewardInfo
{
    public string srcRewards { get; set; }
    public string destRewards { get; set; }
}

public class FeePayerInfo
{
    public string feePayerAccountId { get; set; }
}

public class SystemMessages
{
    public string type { get; set; }
    public string message { get; set; }
}

public class Group
{
    public int th { get; set; }
    public Dictionary<string, string> users { get; set; }
}

public class Transaction
{
    public string id { get; set; }
    public string assetId { get; set; }
    public SourceDestination source { get; set; }
    public SourceDestination destination { get; set; }
    public ContractCallDecodedData contractCallDecodedData { get; set; }
    public int requestedAmount { get; set; }
    public int amount { get; set; }
    public int netAmount { get; set; }
    public int amountUSD { get; set; }
    public int serviceFee { get; set; }
    public int networkFee { get; set; }
    public long createdAt { get; set; }
    public long lastUpdated { get; set; }
    public string status { get; set; }
    public string txHash { get; set; }
    public string tag { get; set; }
    public string subStatus { get; set; }
    public string destinationAddress { get; set; }
    public string sourceAddress { get; set; }
    public string destinationAddressDescription { get; set; }
    public string destinationTag { get; set; }
    public List<string> signedBy { get; set; }
    public string createdBy { get; set; }
    public string rejectedBy { get; set; }
    public string addressType { get; set; }
    public string note { get; set; }
    public string exchangeTxId { get; set; }
    public string feeCurrency { get; set; }
    public string operation { get; set; }
    public List<NetworkRecord> networkRecords { get; set; }
    public AmlScreeningResult amlScreeningResult { get; set; }
    public string customerRefId { get; set; }
    public int numOfConfirmations { get; set; }
    public AmountInfo amountInfo { get; set; }
    public FeeInfo feeInfo { get; set; }
    public SignedMessages signedMessages { get; set; }
    public Dictionary<string, object> extraParameters { get; set; }
    public string externalTxId { get; set; }
    public List<Destination> destinations { get; set; }
    public BlockInfo blockInfo { get; set; }
    public AuthorizationInfo authorizationInfo { get; set; }
    public int index { get; set; }
    public RewardInfo rewardInfo { get; set; }
    public FeePayerInfo feePayerInfo { get; set; }
    public bool treatAsGrossAmount { get; set; }
    public SystemMessages systemMessages { get; set; }
}
