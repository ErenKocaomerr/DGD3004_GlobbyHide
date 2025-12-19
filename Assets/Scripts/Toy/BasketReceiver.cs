using UnityEngine;

public class BasketReceiver : MonoBehaviour
{
    [HideInInspector] // Inspector'da görünmesine gerek yok, Manager ayarlýyor
    public ToyType currentTargetType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        ToyData toy = other.GetComponent<ToyData>();

        if (toy != null)
        {
            // Doðru oyuncaðý mý yakaladýk?
            if (toy.toyType == currentTargetType)
            {
                // DOÐRU: +1 Puan
                MiniGameManager.instance.AddScore(1);

                // Ýstersen burada bir "Ding" sesi çalabilirsin
            }
            else
            {
                // YANLIÞ: -1 Puan (Ceza vermek istiyorsan)
                // Ýstemiyorsan bu satýrý silebilirsin.
                MiniGameManager.instance.AddScore(-1);
            }

            // Yakalanan oyuncaðý yok et
            Destroy(other.gameObject);
        }
    }
}
