namespace MyLib
{
    public class B4B3
    {
        public static string ThangTiengAnh(int thang)
        {
            switch (thang)
            {
                case 1: return "Tieng anh cua thang 1 la January.";
                case 2: return "Tieng anh cua thang 2 la February.";
                case 3: return "Tieng anh cua thang 3 la March.";
                case 4: return "Tieng anh cua thang 4 la April.";
                case 5: return "Tieng anh cua thang 5 la May.";
                case 6: return "Tieng anh cua thang 6 la June.";
                case 7: return "Tieng anh cua thang 7 la July.";
                case 8: return "Tieng anh cua thang 8 la August.";
                case 9: return "Tieng anh cua thang 9 la September.";
                case 10: return "Tieng anh cua thang 10 la October.";
                case 11: return "Tieng anh cua thang 11 la November.";
                case 12: return "Tieng anh cua thang 12 la December.";
                default: return "Thang khong hop le.";
            }
        }
    }
}