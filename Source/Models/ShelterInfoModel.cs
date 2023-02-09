namespace NaturalDisastersRenewal.Models
{
    public class ShelterInfoModel
    {
        public ushort ShelterId;
        public Building BuildingData;
        public ShelterAI ShelterData {
            get {
                return BuildingData.Info?.m_buildingAI as ShelterAI;
            } 
        }
    }
}