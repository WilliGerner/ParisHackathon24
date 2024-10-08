using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorShower : MonoBehaviour
{
    public GameObject meteorPrefab;  // Le prefab de météorite
    public Transform target;         // La cible que les météorites doivent viser
    public float spawnRadius = 50f;  // Le rayon dans lequel les météorites vont apparaître

    // Paramètres pour la vitesse contrôlée par le code (et non la gravité)
    public float minSpeed = 20f;     // Vitesse minimale des météorites
    public float maxSpeed = 50f;     // Vitesse maximale des météorites

    // Paramètres pour la taille aléatoire
    public float minScale = 0.5f;    // Taille minimale des météorites
    public float maxScale = 3.0f;    // Taille maximale des météorites

    // Paramètres pour le délai aléatoire entre chaque spawn
    public float minSpawnInterval = 0.2f; // Délai minimal entre deux météorites
    public float maxSpawnInterval = 1.0f; // Délai maximal entre deux météorites

    // Durée de vie des météorites avant qu'elles ne disparaissent
    public float meteorLifetime = 5f; // Durée de vie en secondes

    // Paramètres pour la vitesse de rotation des météorites
    public float minRotationSpeed = 10f; // Vitesse de rotation minimale
    public float maxRotationSpeed = 100f; // Vitesse de rotation maximale

    void Start()
    {
        StartCoroutine(SpawnMeteors());
    }

    IEnumerator SpawnMeteors()
    {
        while (true) // Boucle infinie pour générer des météorites en continu
        {
            SpawnMeteor();

            // Délai aléatoire entre chaque spawn
            float randomInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(randomInterval);
        }
    }

    void SpawnMeteor()
    {
        // Générer une position aléatoire autour de la cible à l'extérieur du rayon
        Vector3 spawnPosition = GetRandomSpawnPosition(target.position, spawnRadius);

        // Instancier la météorite
        GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);

        // Appliquer une taille aléatoire à la météorite
        float randomScale = Random.Range(minScale, maxScale);
        meteor.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

        // Calculer la direction vers la cible
        Vector3 direction = (target.position - spawnPosition).normalized;

        // Désactiver la gravité pour utiliser la vitesse contrôlée par le code
        Rigidbody rb = meteor.GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Ajouter une force pour diriger la météorite directement vers la cible avec une vitesse aléatoire
        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        rb.velocity = direction * randomSpeed;  // Utiliser la vitesse plutôt que la force

        // Ajouter une rotation aléatoire pour que la météorite tourne sur elle-même
        Vector3 randomRotation = new Vector3(
            Random.Range(-1f, 1f),  // Rotation aléatoire sur l'axe X
            Random.Range(-1f, 1f),  // Rotation aléatoire sur l'axe Y
            Random.Range(-1f, 1f)   // Rotation aléatoire sur l'axe Z
        ).normalized * Random.Range(minRotationSpeed, maxRotationSpeed); // Vitesse de rotation aléatoire

        rb.angularVelocity = randomRotation;  // Appliquer la rotation à la météorite

        // Démarrer la coroutine pour détruire la météorite après sa durée de vie
        StartCoroutine(DestroyMeteorAfterTime(meteor, meteorLifetime));
    }

    // Méthode pour obtenir une position aléatoire dans une sphère autour de la cible
    private Vector3 GetRandomSpawnPosition(Vector3 targetPosition, float radius)
    {
        // Générer une direction aléatoire sur la sphère
        Vector3 randomDirection = Random.onUnitSphere; // Direction aléatoire sur une sphère
        randomDirection.y = Mathf.Abs(randomDirection.y); // Assurer que les météorites apparaissent en dessous de la cible aussi

        // Retourner la position en ajoutant le rayon à la direction aléatoire
        return targetPosition + randomDirection * radius;
    }

    IEnumerator DestroyMeteorAfterTime(GameObject meteor, float lifetime)
    {
        yield return new WaitForSeconds(lifetime); // Attendre la durée de vie spécifiée
        Destroy(meteor); // Détruire la météorite
    }
}


