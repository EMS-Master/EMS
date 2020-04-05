using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TransactionContract;

namespace TransactionManagerService
{
    class TransactionScadaPRProxy : ITransactionContract
    {
        private static ITransactionContract proxy;
        private static DuplexChannelFactory<ITransactionContract> factory;

        public static ITransactionContract Instance
        {
            get
            {
                if (proxy == null)
                {
                    InstanceContext context = new InstanceContext(new TransactionManager());
                    factory = new DuplexChannelFactory<ITransactionContract>(context, "ScadaCRTransactionEndpoint");
                    proxy = factory.CreateChannel();
                }

                return proxy;
            }

            set
            {
                if (proxy == null)
                {
                    proxy = value;
                }
            }
        }

        public bool Commit()
        {
            return proxy.Commit();
        }

        public UpdateResult Prepare(ref Delta delta)
        {
            return proxy.Prepare(ref delta);
        }

        public bool Rollback()
        {
            return proxy.Rollback();
        }
    }
}
