using Garderoba.Model;
using Garderoba.Repository.Common;
using Npgsql;

namespace Garderoba.Repository
{
    public class CostumePartRepository : ICostumePartRepository
    {
        private const string _connectionString = "Host=localhost;Port=5433;Username=postgres;Password=PeLana2606;Database=Garderoba";
    }
}
