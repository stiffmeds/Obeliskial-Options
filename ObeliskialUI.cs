namespace Obeliskial_Options
{
  /*
  public class ObeliskialUI : MonoBehaviour
  {
      public static UIBase uiBase { get; private set; }
      public static RectTransform NavBarRect;
      public static GameObject medsNav;
      public static ButtonRef settingsBtn;
      public static ButtonRef userToolsBtn;
      public static ButtonRef devToolsBtn;
      public static ButtonRef hideBtn;
      public static GameObject lockAtOGO;
      public static Toggle lockAtOToggle;
      public static Text labelMouseX;
      public static Text labelMouseY;
      public static bool ShowUI
      {
          get => uiBase != null && uiBase.Enabled;
          set
          {
              if (uiBase == null || !uiBase.RootObject || uiBase.Enabled == value)
                  return;

              UniversalUI.SetUIActive(PluginInfo.PLUGIN_GUID, value);
          }
      }
      internal static void InitUI()
      {

          uiBase = UniversalUI.RegisterUI(PluginInfo.PLUGIN_GUID, UpdateUI);
          //MedsUI MedsPanel = new MedsUI(uiBase);
          medsNav = UIFactory.CreateUIObject("medsNavbar", uiBase.RootObject);
          UIFactory.SetLayoutGroup<VerticalLayoutGroup>(medsNav, false, false, true, true, 5, 4, 4, 4, 4, TextAnchor.UpperCenter);
          medsNav.AddComponent<Image>().color = new Color(0.03f, 0.008f, 0.05f, 0.9f);
          NavBarRect = medsNav.GetComponent<RectTransform>();
          NavBarRect.pivot = new Vector2(1f, 0.5f);

          NavBarRect.anchorMin = new Vector2(1f, 0.5f);
          NavBarRect.anchorMax = new Vector2(1f, 0.5f);
          NavBarRect.anchoredPosition = new Vector2(NavBarRect.anchoredPosition.x, NavBarRect.anchoredPosition.y);
          NavBarRect.sizeDelta = new(100f, 225f);
          Text title = UIFactory.CreateLabel(medsNav, "Title", "Obeliskial\nOptions\nv" + PluginInfo.PLUGIN_VERSION, TextAnchor.UpperCenter);
          UIFactory.SetLayoutElement(title.gameObject, minWidth: 100);

          // ButtonRef tempBtn = UIFactory.CreateButton(medsNav, "tempButton", "TEST");
          // UIFactory.SetLayoutElement(tempBtn.Component.gameObject, minWidth: 80, minHeight: 30, flexibleWidth: 0);
          // RuntimeHelper.SetColorBlock(tempBtn.Component, new Color(0.22f, 0.54f, 0.22f), new Color(0.15f, 0.71f, 0.1f), new Color(0.08f, 0.5f, 0.06f));

          // tempBtn.OnClick += CaptureEvent;

          labelMouseX = UIFactory.CreateLabel(medsNav, "labelMouseX", "x:", TextAnchor.UpperLeft);
          UIFactory.SetLayoutElement(labelMouseX.gameObject, minWidth: 100);

          labelMouseY = UIFactory.CreateLabel(medsNav, "labelMouseY", "y:", TextAnchor.UpperLeft);
          UIFactory.SetLayoutElement(labelMouseY.gameObject, minWidth: 100);

          //settingsBtn = UIFactory.CreateButton(medsNav, "settingsButton", "Settings");
          //UIFactory.SetLayoutElement(settingsBtn.Component.gameObject, minWidth: 85, minHeight: 30, flexibleWidth: 0);


          //userToolsBtn = UIFactory.CreateButton(medsNav, "userToolsBtn", "User Tools");
          //UIFactory.SetLayoutElement(userToolsBtn.Component.gameObject, minWidth: 85, minHeight: 30, flexibleWidth: 0);

          //devToolsBtn = UIFactory.CreateButton(medsNav, "devToolsBtn", "Dev Tools");
          //UIFactory.SetLayoutElement(devToolsBtn.Component.gameObject, minWidth: 85, minHeight: 30, flexibleWidth: 0);


          //hideBtn = UIFactory.CreateButton(medsNav, "hideBtn", "Hide (F1)");
          //UIFactory.SetLayoutElement(hideBtn.Component.gameObject, minWidth: 85, minHeight: 30, flexibleWidth: 0);

          lockAtOGO = UIFactory.CreateToggle(medsNav, "disableButtonsToggle", out lockAtOToggle, out Text lockAtOText);
          lockAtOText.text = "Lock AtO";
          lockAtOToggle.isOn = false;
          UIFactory.SetLayoutElement(lockAtOGO, minWidth: 85, minHeight: 20);
          //medsNav.

          Canvas.ForceUpdateCanvases();
          ShowUI = true;
          UniversalUI.SetUIActive(PluginInfo.PLUGIN_GUID, true);
          Options.Log.LogInfo($"UI... created?!");
      }
      internal static void UpdateUI()
      {
          //float mX = Input.mousePosition.x;
          //float mY = Input.mousePosition.y;
          Vector3 newPos = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
          labelMouseX.text = "x:" + newPos.x.ToString();
          labelMouseY.text = "y:" + newPos.y.ToString();
      }

      void OnGUI()
      {
          Options.Log.LogInfo(Event.current.ToString());
      }

      static void CaptureEvent()
      {
          Options.Log.LogInfo(Event.current.ToString());
          Event.current.Use();
          Options.Log.LogInfo(Event.current.ToString());
      }

  }
  /*public class SettingsPanel : UniverseLib.UI.Panels.PanelBase
  {
      public SettingsPanel(UIBase owner) : base(owner) { }
      public override string Name => "Mod Versions (F1 to hide)";
      public override int MinWidth => 300;
      public override int MinHeight => 200;
      public override Vector2 DefaultAnchorMin => new(1.35f, 1.7f);
      public override Vector2 DefaultAnchorMax => new(1.35f, 1.7f);
      public override bool CanDragAndResize => true;

      protected override void ConstructPanelContent()
      {
          ModVersionUI.modVersions = UIFactory.CreateLabel(ContentRoot, "Mod Versions", "Obeliskial\nEssentials\nv" + PluginInfo.PLUGIN_VERSION, TextAnchor.UpperLeft);
          UIFactory.SetLayoutElement(ModVersionUI.modVersions.gameObject);
      }
  }*/
}