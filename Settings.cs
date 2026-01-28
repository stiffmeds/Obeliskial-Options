using UnityEngine;

namespace Obeliskial_Options
{
  public class Settings
  {
    public static void SettingsSetup()
    {
      // I've left this because it isn't called anywhere.
      // it just recreates the Gameplay tab of the Settings menu.
      // I think the real answer is actually using unity :D
      // otherwise I will have no realistic clue how the fuck to implement things like scrollbars :(
      Transform gameplayTab = SettingsManager.Instance.gameplayTab;
      Transform modTab = UnityEngine.Object.Instantiate<Transform>(gameplayTab, ((Component)gameplayTab).transform.parent);
      ((Component)modTab).transform.SetSiblingIndex(((Component)gameplayTab).transform.GetSiblingIndex() + 1);
      ((UnityEngine.Object)modTab).name = "modTab";
      //((UnityEventBase)modTab.onValueChanged).RemoveAllListeners();
      //tmpDropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();
      RectTransform component = ((Component)modTab).GetComponent<RectTransform>();
      Rect rect = component.rect;
      double width = (double)((Rect)rect).width;
      component.anchoredPosition = new Vector2((float)((double)component.anchoredPosition.x + width + 10.0), component.anchoredPosition.y);
      modTab.gameObject.SetActive(true);
      //tmpDropdown.ClearOptions();
      //Plugin.languagesDropdown = tmpDropdown;
    }
  }

}