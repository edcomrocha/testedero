using AutoMapper;
using Miningcore.Api.Responses;
using Miningcore.Blockchain;
using Miningcore.Blockchain.Alephium.Configuration;
using Miningcore.Blockchain.Ergo.Configuration;
using Miningcore.Blockchain.Kaspa.Configuration;
using Miningcore.Configuration;
using Miningcore.Extensions;
using Miningcore.Mining;
using PoolStats = Miningcore.Persistence.Model.PoolStats;

namespace Miningcore.Api.Extensions;

public static class MiningPoolExtensions
{
    public static PoolInfo ToPoolInfo(this PoolConfig poolConfig, IMapper mapper, PoolStats stats, IMiningPool pool)
    {
        var poolInfo = mapper.Map<PoolInfo>(poolConfig);

        poolInfo.PoolStats = mapper.Map<Mining.PoolStats>(stats);
        poolInfo.NetworkStats = pool?.NetworkStats ?? mapper.Map<BlockchainStats>(stats);

        // pool wallet link
        var addressInfobaseUrl = poolConfig.Template.ExplorerAccountLink;
        if(!string.IsNullOrEmpty(addressInfobaseUrl))
            poolInfo.AddressInfoLink = string.Format(addressInfobaseUrl, poolInfo.Address);

        // pool fees
        poolInfo.PoolFeePercent = poolConfig.RewardRecipients != null ? (float) poolConfig.RewardRecipients.Sum(x => x.Percentage) : 0;

        // strip security critical stuff
        if(poolInfo.PaymentProcessing.Extra != null)
        {
            var extra = poolInfo.PaymentProcessing.Extra;

            switch(poolInfo.Coin.Family)
            {
                case "alephium":
                    extra.StripValue(nameof(AlephiumPaymentProcessingConfigExtra.WalletPassword));
                    break;
                case "ergo":
                    extra.StripValue(nameof(ErgoPaymentProcessingConfigExtra.WalletPassword));
                    break;
                case "kaspa":
                    extra.StripValue(nameof(KaspaPaymentProcessingConfigExtra.WalletPassword));
                    break;
            }
        }

        if(poolInfo.Ports != null)
            foreach(var port in poolInfo.Ports.Keys)
            {
                var portInfo = poolInfo.Ports[port];

                portInfo.TlsPfxFile = null;
                portInfo.TlsPfxPassword = null;
            }
        return poolInfo;
    }
}
