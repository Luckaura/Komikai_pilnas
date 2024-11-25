# Komikų pasirodymų atsiliepimų svetainė

##  Sistemos paskirtis
Šios sistemos paskirtis yra suteikti galimybę komikų mėgėjams rašyti atsiliepimus apie įvykusius komikų pasirodymus, kuriuos galės skaityti ir kiti naudotojai.

## Funkciniai reikalavimai 

Reikalavimai yra suskirstyti pagal tris roles:<br/>
Svetainės administratorius:
-	Peržiūrėti komikų sąrašą
-	Pridėti komiką prie komikų sąrašo
-	Pašalinti komiką iš komikų sąrašo
-	Redaguoti informaciją apie komikus
-	Peržiūrėti registruotų naudotojų sąrašą
-	Šalinti registruotus naudotojus
-	Peržiūrėti atsiliepimus apie renginius
-	Pašalinti atsiliepimą apie renginį
-	Pridėti renginį
-	Pašalinti renginį
-	Redaguoti informaciją apie renginį
-	Peržiūrėti informaciją apie renginį
-	Pašalinti atsiliepimą apie renginį

Neregistruotas naudotojas:
-	Peržiūrėti atsiliepimus apie renginius
-	Peržiūrėti informaciją apie komikus
-	Prisiregistruoti prie aplikacijos
-	Prisijungti prie aplikacijos

Registruotas naudotojas:
-	Peržiūrėti informaciją apie komikus
-	Peržiūrėti atsiliepimus apie renginius
-	Rašyti atsiliepimą apie renginį
-	Pašalinti savo atsiliepimą apie renginį
-	Redaguoti savo atsiliepimą apie renginį
-	Pridėti komiką į mėgstamų komikų sąrašą
-	Peržiūrėti savo mėgstamų komikų sąrašą
-	Pašalinti komikus iš mėgstamų komikų sąrašo
-	Prisijungti prie aplikacijos
-	Atsijungti nuo aplikacijos
-	Peržiūrėti savo paskyrą
-	Pakeisti savo slapyvardį
-	Ištrinti savo paskyrą
## Hierarchinis ryšys
Taikomosios srities objektai tarpusavyje susieti prasminiu ir hierarchiniu ryšiu: Komikas -> Pasirodymas -> Komentaras

## Pasirinktų technologijų aprašymas

Kliento pusė (angl. Front-End) — Vue.js, nes yra lengvai pritaikomas, paprastesnis ir draugiškesnis naujiems naudotojams. Taip pat su juo galima kurti dinamines vartotojo sąsajas, leidžiančias greitai atnaujinti turinį be poreikio perkrauti puslapį, kas yra aktualu, kai yra rašomi komentarai.<br/>
Serverio pusė (angl. Back-End) — .NET Core, nes lengvai integruojasi su įvairiomis duomenų bazių sistemomis ir yra žinoma technologija.<br/>
Duomenų bazė — PostgreSQL, nes yra viena populiariausių ir plačiausiai naudojamų atvirojo kodo reliacinių duomenų bazių valdymo sistemų.<br/>
