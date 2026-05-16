namespace com.RADIO.Datinate.RMVC.Shared
{

    /**
     * Contains 3 DatVOs. None will contain the same Roms, 
     * but all ROMs will be present across the three dtos
     */
    public class DatComparisonVO 
    {
        public DatVO leftDatVO;
        public DatVO rightDatVO;
        public DatVO diffDatVO;

        public DatComparisonVO(DatVO leftDatVO, DatVO rightDatVO, DatVO diffDatVO) 
        {
            this.leftDatVO = leftDatVO;
            this.rightDatVO = rightDatVO;
            this.diffDatVO = diffDatVO;
        }
    }
}
