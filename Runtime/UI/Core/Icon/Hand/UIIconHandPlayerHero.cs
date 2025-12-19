
namespace GGemCo2DTcg
{
    public class UIIconHandPlayerHero : UIIconHandPlayer
    {
        public override bool ChangeInfoByUid(int cardUid, int iconCount = 0, int iconLevel = 0,
            bool iconIsLearn = false, int remainCoolTime = 0)
        {
            if (!base.ChangeInfoByUid(cardUid, iconCount, iconLevel, iconIsLearn, remainCoolTime)) return false;
            
            // 영웅은 마나 이미지 안보이도록 처리
            if (imageMana != null) 
            {
                imageMana.gameObject.SetActive(false);
            }

            return true;
        }
    }
}