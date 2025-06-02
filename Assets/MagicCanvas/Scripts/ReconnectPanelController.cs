using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReconnectPanelController : MonoBehaviour
{
    public GameObject reconnectPanel;
    public Text statusText;

    private Coroutine flickerCoroutine;

    public void ShowFlicker()
    {
        reconnectPanel.SetActive(true);
        statusText.text = "�w�_�u�A���խ��s�s�u��";
        flickerCoroutine = StartCoroutine(FlickerText());
    }

    public void ShowSuccessAndHide()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }

        statusText.text = "���s�s�u���\";
        StartCoroutine(AutoHideAfterDelay(1f)); // 1���۰�����
    }

    private IEnumerator FlickerText()
    {
        while (true)
        {
            statusText.enabled = !statusText.enabled;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator AutoHideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        reconnectPanel.SetActive(false);
    }
    public void CancelAndHideImmediately()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }

        StopAllCoroutines(); // �O�I�]�� AutoHide ��{����
        reconnectPanel.SetActive(false);
    }
}
