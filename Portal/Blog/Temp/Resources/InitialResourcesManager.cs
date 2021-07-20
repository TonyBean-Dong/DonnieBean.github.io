using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class InitialResourcesManager : MonoBehaviour
{
    #region ResourceBuilderAttributes

    public static GameObject Game{ get { return Resources.Load<GameObject>("Game"); } }
    public static Sprite img_LeShiLoding{ get { return Resources.Load<Sprite>("img_LeShiLoding"); } }
    public static AudioClip aud_boing{ get { return Resources.Load<AudioClip>("Audio/aud_boing"); } }
    public static AudioClip aud_CustomerService{ get { return Resources.Load<AudioClip>("Audio/aud_CustomerService"); } }
    public static AudioClip aud_Event{ get { return Resources.Load<AudioClip>("Audio/aud_Event"); } }
    public static AudioClip aud_PersonalCenter{ get { return Resources.Load<AudioClip>("Audio/aud_PersonalCenter"); } }
    public static AudioClip aud_Promotion{ get { return Resources.Load<AudioClip>("Audio/aud_Promotion"); } }
    public static AudioClip aud_Recharge{ get { return Resources.Load<AudioClip>("Audio/aud_Recharge"); } }
    public static AudioClip aud_Safe{ get { return Resources.Load<AudioClip>("Audio/aud_Safe"); } }
    public static AudioClip aud_StartGame{ get { return Resources.Load<AudioClip>("Audio/aud_StartGame"); } }
    public static AudioClip aud_VipOrBankCard{ get { return Resources.Load<AudioClip>("Audio/aud_VipOrBankCard"); } }
    public static AudioClip aud_WashCode{ get { return Resources.Load<AudioClip>("Audio/aud_WashCode"); } }
    public static AudioClip aud_Withdraw{ get { return Resources.Load<AudioClip>("Audio/aud_Withdraw"); } }
    public static Sprite img_Bind{ get { return Resources.Load<Sprite>("Image/img_Bind"); } }
    public static Sprite img_BindTips{ get { return Resources.Load<Sprite>("Image/img_BindTips"); } }
    public static Sprite img_Bonus{ get { return Resources.Load<Sprite>("Image/img_Bonus"); } }
    public static Sprite img_CannotReceive{ get { return Resources.Load<Sprite>("Image/img_CannotReceive"); } }
    public static Sprite img_canReceive{ get { return Resources.Load<Sprite>("Image/img_canReceive"); } }
    public static Sprite img_Check{ get { return Resources.Load<Sprite>("Image/img_Check"); } }
    public static Sprite img_ChongZhiChenGong{ get { return Resources.Load<Sprite>("Image/img_ChongZhiChenGong"); } }
    public static Sprite img_CopySuccess{ get { return Resources.Load<Sprite>("Image/img_CopySuccess"); } }
    public static Sprite img_Delete{ get { return Resources.Load<Sprite>("Image/img_Delete"); } }
    public static Sprite[] img_gr_V = new Sprite[11];
    public static Sprite[] img_gr_VIP = new Sprite[11];
    public static Sprite[] img_gr_VIPIcon = new Sprite[11];
    public static Sprite[] img_gr_viptitle = new Sprite[11];
    public static Sprite[] img_gr_woman = new Sprite[7];
    public static Sprite img_Logo{ get { return Resources.Load<Sprite>("Image/img_Logo"); } }
    public static Sprite img_Plus{ get { return Resources.Load<Sprite>("Image/img_Plus"); } }
    public static Sprite img_Readed{ get { return Resources.Load<Sprite>("Image/img_Readed"); } }
    public static Sprite img_Send{ get { return Resources.Load<Sprite>("Image/img_Send"); } }
    public static Sprite img_ShortLine{ get { return Resources.Load<Sprite>("Image/img_ShortLine"); } }
    public static Sprite img_SuccessIcon{ get { return Resources.Load<Sprite>("Image/img_SuccessIcon"); } }
    public static Sprite img_TimeOut{ get { return Resources.Load<Sprite>("Image/img_TimeOut"); } }
    public static Sprite img_TiShi{ get { return Resources.Load<Sprite>("Image/img_TiShi"); } }
    public static Sprite img_unread{ get { return Resources.Load<Sprite>("Image/img_unread"); } }
    public static Sprite img_WantToDelete{ get { return Resources.Load<Sprite>("Image/img_WantToDelete"); } }
    public static Sprite img_WarningIcon{ get { return Resources.Load<Sprite>("Image/img_WarningIcon"); } }
    public static Sprite img_Wifi{ get { return Resources.Load<Sprite>("Image/img_Wifi"); } }
    public static TextAsset txt_JsTest{ get { return Resources.Load<TextAsset>("Text/txt_JsTest"); } }
    public static AudioClip[] aud_gr_Start = new AudioClip[6];
    public static GameObject TwoColorTableElement1{ get { return Resources.Load<GameObject>("GameObject/Common/TwoColorTableElement1"); } }
    public static GameObject TwoColorTableElement2{ get { return Resources.Load<GameObject>("GameObject/Common/TwoColorTableElement2"); } }
    public static GameObject TwoColorTableElement2_1{ get { return Resources.Load<GameObject>("GameObject/Common/TwoColorTableElement2_1"); } }
    public static GameObject TwoColorTableElement2_2{ get { return Resources.Load<GameObject>("GameObject/Common/TwoColorTableElement2_2"); } }
    public static GameObject EventButton{ get { return Resources.Load<GameObject>("GameObject/Event/EventButton"); } }
    public static GameObject EventDetails{ get { return Resources.Load<GameObject>("GameObject/Event/EventDetails"); } }
    public static GameObject EventElement{ get { return Resources.Load<GameObject>("GameObject/Event/EventElement"); } }
    public static GameObject EventPanel{ get { return Resources.Load<GameObject>("GameObject/Event/EventPanel"); } }
    public static GameObject GamePanelContent{ get { return Resources.Load<GameObject>("GameObject/MainMenu/GamePanelContent"); } }
    public static GameObject GamePanelContentSpecial{ get { return Resources.Load<GameObject>("GameObject/MainMenu/GamePanelContentSpecial"); } }
    public static GameObject MainMenuButton{ get { return Resources.Load<GameObject>("GameObject/MainMenu/MainMenuButton"); } }
    public static GameObject SecendLevelMenuButton{ get { return Resources.Load<GameObject>("GameObject/MainMenu/SecendLevelMenuButton"); } }
    public static GameObject MessageContent{ get { return Resources.Load<GameObject>("GameObject/Message/MessageContent"); } }
    public static GameObject MessageElement{ get { return Resources.Load<GameObject>("GameObject/Message/MessageElement"); } }
    public static GameObject PersonalCenterTableContent{ get { return Resources.Load<GameObject>("GameObject/PersonalCenter/PersonalCenterTableContent"); } }
    public static GameObject btn_RebateTable{ get { return Resources.Load<GameObject>("GameObject/Promotion/btn_RebateTable"); } }
    public static GameObject PromotionTableElement{ get { return Resources.Load<GameObject>("GameObject/Promotion/PromotionTableElement"); } }
    public static GameObject RebateTableMask{ get { return Resources.Load<GameObject>("GameObject/Promotion/RebateTableMask"); } }
    public static GameObject btn_MoneyNum{ get { return Resources.Load<GameObject>("GameObject/Recharge/btn_MoneyNum"); } }
    public static GameObject btn_PayChannel{ get { return Resources.Load<GameObject>("GameObject/Recharge/btn_PayChannel"); } }
    public static GameObject btn_RechargeType{ get { return Resources.Load<GameObject>("GameObject/Recharge/btn_RechargeType"); } }
    public static GameObject panel_BankCard{ get { return Resources.Load<GameObject>("GameObject/Recharge/panel_BankCard"); } }
    public static GameObject panel_VIP{ get { return Resources.Load<GameObject>("GameObject/Recharge/panel_VIP"); } }
    public static GameObject RechageLogTableElement{ get { return Resources.Load<GameObject>("GameObject/Recharge/RechageLogTableElement"); } }
    public static GameObject RechargeSubPanel{ get { return Resources.Load<GameObject>("GameObject/Recharge/RechargeSubPanel"); } }
    public static GameObject SafeDetailPage{ get { return Resources.Load<GameObject>("GameObject/SafePanel/SafeDetailPage"); } }
    public static GameObject SafeDetailTableContent{ get { return Resources.Load<GameObject>("GameObject/SafePanel/SafeDetailTableContent"); } }
    public static GameObject TipsBox{ get { return Resources.Load<GameObject>("GameObject/Tips/TipsBox"); } }
    public static GameObject TipsLable{ get { return Resources.Load<GameObject>("GameObject/Tips/TipsLable"); } }
    public static GameObject TipsPanel{ get { return Resources.Load<GameObject>("GameObject/Tips/TipsPanel"); } }
    public static GameObject TipsText{ get { return Resources.Load<GameObject>("GameObject/Tips/TipsText"); } }
    public static GameObject WashCodeButton{ get { return Resources.Load<GameObject>("GameObject/WashCode/WashCodeButton"); } }
    public static GameObject WashCodeList{ get { return Resources.Load<GameObject>("GameObject/WashCode/WashCodeList"); } }
    public static GameObject WashCodeLogTableContent{ get { return Resources.Load<GameObject>("GameObject/WashCode/WashCodeLogTableContent"); } }
    public static GameObject WashCodeProportionTableContent{ get { return Resources.Load<GameObject>("GameObject/WashCode/WashCodeProportionTableContent"); } }
    public static GameObject WashCodeTableContent_1{ get { return Resources.Load<GameObject>("GameObject/WashCode/WashCodeTableContent_1"); } }
    public static GameObject WashCodeTableContent_2{ get { return Resources.Load<GameObject>("GameObject/WashCode/WashCodeTableContent_2"); } }
    public static GameObject BankCardGridContent{ get { return Resources.Load<GameObject>("GameObject/Withdraw/BankCardGridContent"); } }
    public static GameObject CapitalFlowTableContent{ get { return Resources.Load<GameObject>("GameObject/Withdraw/CapitalFlowTableContent"); } }
    public static GameObject ge_ChioceCard{ get { return Resources.Load<GameObject>("GameObject/Withdraw/ge_ChioceCard"); } }
    public static GameObject Panel_WithdrawBandCard{ get { return Resources.Load<GameObject>("GameObject/Withdraw/Panel_WithdrawBandCard"); } }
    public static GameObject WithDrawList{ get { return Resources.Load<GameObject>("GameObject/Withdraw/WithDrawList"); } }
    public static GameObject WithdrawLogTableContent{ get { return Resources.Load<GameObject>("GameObject/Withdraw/WithdrawLogTableContent"); } }

    #endregion ResourceBuilderAttributes

    private void Awake()
	{
        #region ResourceBuilderAwake

        img_gr_V[1] = Resources.Load<Sprite>("Image/img_gr_V1");
        img_gr_V[10] = Resources.Load<Sprite>("Image/img_gr_V10");
        img_gr_V[2] = Resources.Load<Sprite>("Image/img_gr_V2");
        img_gr_V[3] = Resources.Load<Sprite>("Image/img_gr_V3");
        img_gr_V[4] = Resources.Load<Sprite>("Image/img_gr_V4");
        img_gr_V[5] = Resources.Load<Sprite>("Image/img_gr_V5");
        img_gr_V[6] = Resources.Load<Sprite>("Image/img_gr_V6");
        img_gr_V[7] = Resources.Load<Sprite>("Image/img_gr_V7");
        img_gr_V[8] = Resources.Load<Sprite>("Image/img_gr_V8");
        img_gr_V[9] = Resources.Load<Sprite>("Image/img_gr_V9");
        img_gr_VIP[1] = Resources.Load<Sprite>("Image/img_gr_VIP1");
        img_gr_VIP[10] = Resources.Load<Sprite>("Image/img_gr_VIP10");
        img_gr_VIP[2] = Resources.Load<Sprite>("Image/img_gr_VIP2");
        img_gr_VIP[3] = Resources.Load<Sprite>("Image/img_gr_VIP3");
        img_gr_VIP[4] = Resources.Load<Sprite>("Image/img_gr_VIP4");
        img_gr_VIP[5] = Resources.Load<Sprite>("Image/img_gr_VIP5");
        img_gr_VIP[6] = Resources.Load<Sprite>("Image/img_gr_VIP6");
        img_gr_VIP[7] = Resources.Load<Sprite>("Image/img_gr_VIP7");
        img_gr_VIP[8] = Resources.Load<Sprite>("Image/img_gr_VIP8");
        img_gr_VIP[9] = Resources.Load<Sprite>("Image/img_gr_VIP9");
        img_gr_VIPIcon[1] = Resources.Load<Sprite>("Image/img_gr_VIPIcon1");
        img_gr_VIPIcon[10] = Resources.Load<Sprite>("Image/img_gr_VIPIcon10");
        img_gr_VIPIcon[2] = Resources.Load<Sprite>("Image/img_gr_VIPIcon2");
        img_gr_VIPIcon[3] = Resources.Load<Sprite>("Image/img_gr_VIPIcon3");
        img_gr_VIPIcon[4] = Resources.Load<Sprite>("Image/img_gr_VIPIcon4");
        img_gr_VIPIcon[5] = Resources.Load<Sprite>("Image/img_gr_VIPIcon5");
        img_gr_VIPIcon[6] = Resources.Load<Sprite>("Image/img_gr_VIPIcon6");
        img_gr_VIPIcon[7] = Resources.Load<Sprite>("Image/img_gr_VIPIcon7");
        img_gr_VIPIcon[8] = Resources.Load<Sprite>("Image/img_gr_VIPIcon8");
        img_gr_VIPIcon[9] = Resources.Load<Sprite>("Image/img_gr_VIPIcon9");
        img_gr_viptitle[1] = Resources.Load<Sprite>("Image/img_gr_viptitle1");
        img_gr_viptitle[10] = Resources.Load<Sprite>("Image/img_gr_viptitle10");
        img_gr_viptitle[2] = Resources.Load<Sprite>("Image/img_gr_viptitle2");
        img_gr_viptitle[3] = Resources.Load<Sprite>("Image/img_gr_viptitle3");
        img_gr_viptitle[4] = Resources.Load<Sprite>("Image/img_gr_viptitle4");
        img_gr_viptitle[5] = Resources.Load<Sprite>("Image/img_gr_viptitle5");
        img_gr_viptitle[6] = Resources.Load<Sprite>("Image/img_gr_viptitle6");
        img_gr_viptitle[7] = Resources.Load<Sprite>("Image/img_gr_viptitle7");
        img_gr_viptitle[8] = Resources.Load<Sprite>("Image/img_gr_viptitle8");
        img_gr_viptitle[9] = Resources.Load<Sprite>("Image/img_gr_viptitle9");
        img_gr_woman[1] = Resources.Load<Sprite>("Image/img_gr_woman1");
        img_gr_woman[2] = Resources.Load<Sprite>("Image/img_gr_woman2");
        img_gr_woman[3] = Resources.Load<Sprite>("Image/img_gr_woman3");
        img_gr_woman[4] = Resources.Load<Sprite>("Image/img_gr_woman4");
        img_gr_woman[5] = Resources.Load<Sprite>("Image/img_gr_woman5");
        img_gr_woman[6] = Resources.Load<Sprite>("Image/img_gr_woman6");
        aud_gr_Start[1] = Resources.Load<AudioClip>("Audio/StartAudio/aud_gr_Start1");
        aud_gr_Start[2] = Resources.Load<AudioClip>("Audio/StartAudio/aud_gr_Start2");
        aud_gr_Start[3] = Resources.Load<AudioClip>("Audio/StartAudio/aud_gr_Start3");
        aud_gr_Start[4] = Resources.Load<AudioClip>("Audio/StartAudio/aud_gr_Start4");
        aud_gr_Start[5] = Resources.Load<AudioClip>("Audio/StartAudio/aud_gr_Start5");

        #endregion ResourceBuilderAwake
	}
}
