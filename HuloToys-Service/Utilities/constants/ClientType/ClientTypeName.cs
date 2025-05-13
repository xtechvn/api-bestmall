namespace HuloToys_Service.Utilities.constants.ClientType
{
    public static class ClientTypeName
    {
        public static readonly Dictionary<Int16, string> service = new Dictionary<Int16, string>
        {
            {Convert.ToInt16(ClientType.DAILYC1), "DL" },
            {Convert.ToInt16(ClientType.KL), "KL" },
            {Convert.ToInt16(ClientType.DOI_TAC_CL), "CL" },
            {Convert.ToInt16(ClientType.DOANH_NGHIEP), "DN" },
            {Convert.ToInt16(ClientType.CTV), "CTV" }
        };
    }
}
