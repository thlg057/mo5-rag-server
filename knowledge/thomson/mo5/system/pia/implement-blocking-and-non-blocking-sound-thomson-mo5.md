# Implémenter du son bloquant et non bloquant sur Thomson MO5

## Goal

Présenter les capacités sonores du MO5 et deux approches : effets bloquants simples et son non bloquant piloté par VBL/timer.

## Capacités sonores du MO5

- Sortie son de base : **un bit** via la PIA (bip simple).
- Optionnel : **DAC 6 bits** via le port cartouche.

## Son bloquant (simple)

Principe :
- toggler le bit son dans une boucle serrée pour générer un BIP ou une explosion.
- la temporisation est faite par des boucles d’attente en assembleur.

Inconvénient majeur :
- cette approche **bloque totalement le CPU** pendant la durée du son.
- à n’utiliser que pour des sons très courts ou dans des contextes non interactifs.

## Son non bloquant (avancé)

Objectif : produire du son pendant que le jeu continue à tourner.

Approche recommandée :
- créer un petit **player** appelé dans l’interruption VBL (ou un timer) :
  - à chaque VBL, avancer d’un pas dans une séquence musicale ou une table d’effets,
  - mettre à jour le bit son (ou la sortie DAC) selon la valeur courante.
- stocker la partition ou les effets dans des tables en ROM.

Avantages :
- le jeu reste réactif pendant la lecture.
- le coût CPU est réparti frame par frame.

## Conseils de conception

- Si l’on souhaite de la musique de fond, prévoir le player VBL **dès le début du projet**.
- Tester le budget CPU combiné :
  - affichage + logique + son VBL.
- Pour des effets très riches, on peut utiliser des techniques de type **PWM** sur le bit son, au prix d’un surcoût CPU.

Source: `mo5-docs/mo5/guide-developpement-MO5.md`

