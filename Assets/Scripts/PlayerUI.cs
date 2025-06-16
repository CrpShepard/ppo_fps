using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Panel")]
    public Image armorPanel;
    public Image ammoPanel;

    [Header("Text")]
    public TextMeshProUGUI healthDisplay;
    public TextMeshProUGUI armorDisplay;
    public TextMeshProUGUI ammoMagDisplay;
    public TextMeshProUGUI ammoCurrentDisplay;
    public TextMeshProUGUI timerCountdown;

    [Header("Text2")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI ammoText;

    public void SetHealth(int health)
    {
        healthDisplay.text = health.ToString();
    }

    public void SetArmor(int armor)
    {
        armorDisplay.text = armor.ToString();
        if (armor > 0) { ShowArmor(); }
        else { HideArmor(); }
    }

    public void SetAmmo(int ammoMag, int ammoCurrent)
    {
        ammoMagDisplay.text = ammoMag.ToString();
        ammoCurrentDisplay.text = ammoCurrent.ToString();
    }

    public void HideArmor()
    {
        armorDisplay.enabled = false;
        armorText.enabled = false;
        armorPanel.enabled = false;
    }

    public void ShowArmor()
    {
        armorDisplay.enabled = true;
        armorText.enabled = true;
        armorPanel.enabled = true;
    }

    public void HideAmmoCrowbar()
    {
        ammoMagDisplay.enabled = false;
        ammoCurrentDisplay.enabled = false;
        ammoText.enabled = false;
        ammoPanel.enabled = false;
    }

    public void HideAmmoGrenade()
    {
        ammoMagDisplay.enabled = true;
        ammoCurrentDisplay.enabled = false;
        ammoText.enabled = true;
        ammoPanel.enabled = true;
    }

    public void ShowAmmo()
    {
        ammoMagDisplay.enabled = true;
        ammoCurrentDisplay.enabled = true;
        ammoText.enabled = true;
        ammoPanel.enabled = true;
    }

    public void SetTimerCountdown(int time)
    {
        if (time % 60 < 10)
            timerCountdown.text = (time / 60).ToString() + ":0" + (time % 60).ToString();
        else
            timerCountdown.text = (time / 60).ToString() + ":" + (time % 60).ToString();
    }

    private void Start()
    {
        armorPanel.enabled = false;
        ammoPanel.enabled = false;
    }

}
