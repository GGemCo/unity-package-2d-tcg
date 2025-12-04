using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 상대 AI의 카드가 있는 윈도우
    /// </summary>
    public class UIWindowTcgEnemy : UIWindow
    {
        
        protected override void Awake()
        {
            if (TableLoaderManager.Instance == null)
            {
                return;
            }
            if (containerIcon == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgEnemy)}: containerIcon 이 null 입니다.");
                return;
            }
            
            uid = UIWindowConstants.WindowUid.TcgEnemy;

            base.Awake();

            IconPoolManager.SetSetIconHandler(new SetIconHandlerEnemy());
        }

        public override void OnShow(bool show)
        {
            DetachAllIcons();
        }
    }
}