# Proposition d'évolution — Modèle tarifaire TaxiDjibouti

> **Statut : proposition** — à discuter avec le lead avant toute implémentation.
> Le MVP actuel utilise la **matrice zone-à-zone** (voir « État actuel »). Ce document
> trace une évolution possible vers un modèle plus juste, sans l'imposer.

## Contexte

La sous-tâche « CRUD ZonePrice » a livré la gestion admin des tarifs sur le modèle
**matrice zone-à-zone** existant (`ZonePrice(FromZone, ToZone, Price)`). Ce document
capture une réflexion sur les limites de ce modèle et les alternatives, pour une
future US pricing.

## Le piège : zones administratives ≠ zones tarifaires

Les découpages administratifs de Djibouti (3 communes / 6 arrondissements) sont un
mauvais découpage pour la tarification :

- **Balbala** : ~400 000 habitants sur une grande superficie. Un tarif fixe « intra-Balbala »
  ferait payer pareil une course de 1 km et une de 8 km — injuste, et les chauffeurs
  refuseraient les longues courses.
- **Plateau / centre-ville** : minuscule à l'inverse.

Les arrondissements sont trop hétérogènes en taille pour servir de zones tarifaires directes.

## Les 3 approches classiques en ride-hailing

### 1. Distance + temps (modèle Uber/Bolt/Yango)
`tarif_base + prix_km × distance + prix_min × durée`, avec éventuel multiplicateur de demande.
- ✅ Le plus juste et le plus scalable. Calcul de distance faisable avec le stack actuel (PostGIS + NetTopologySuite).
- ⚠️ Le marché djiboutien est habitué au **prix négocié/forfaitaire** ; un compteur qui monte peut générer de la méfiance au début.

### 2. Matrice zone-à-zone (forfait A→B) — **modèle actuel du MVP**
N zones, grille de prix fixes entre chaque paire.
- ✅ Prévisible, inspire confiance, colle à la culture tarifaire locale.
- ⚠️ Se dégrade mal quand N augmente (matrice N×N à maintenir) ; grossier aux frontières de zones.

### 3. Hybride — **recommandé à terme**
Moteur distance+temps comme base, avec des **zones spéciales** par-dessus pour les surcharges :
aéroport Ambouli, port, gare de Nagad, éventuellement une majoration « traversée » pour
les longues courses Balbala ↔ centre.
- ✅ Garde la justesse du distanciel tout en captant les cas particuliers.

## Implémentation cible (hybride)

- Stocker les zones comme **polygones PostGIS** (`geometry(Polygon, 4326)`).
- Matching point → zone via `ST_Contains` / `ST_Intersects`.
- Permet de **redessiner les zones tarifaires indépendamment du découpage administratif**
  (ex. découper Balbala en 3-4 sous-zones réalistes) sans être prisonnier des arrondissements.

## État actuel (livré)

- Entité `ZonePrice(FromZone, ToZone, Price)` — matrice zone-à-zone, zones nommées (strings).
- Index unique `(FromZone, ToZone)`, prix par défaut 1000 FDJ.
- CRUD admin (`/api/admin/zone-prices`) : create / update / delete / liste.
- Zones connues (frontend) : Centre-ville, Balbala, Aéroport, Héron.

## Chemin de migration suggéré

1. Court terme : peupler la matrice avec les vrais forfaits (cette sous-tâche le permet).
2. Moyen terme : introduire les zones-polygones PostGIS + moteur distance/temps en parallèle.
3. Bascule progressive vers l'hybride, la matrice servant de forfaits/surcharges par-dessus.
