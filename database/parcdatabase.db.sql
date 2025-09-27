BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Agents" (
	"idrh"	TEXT,
	"nom"	TEXT,
	"prenom"	TEXT,
	"email"	TEXT,
	"equipe"	TEXT,
	"heberge"	INTEGER,
	"commentaire"	TEXT,
	"nom_equipe"	TEXT,
	PRIMARY KEY("idrh"),
	FOREIGN KEY("nom_equipe") REFERENCES "Equipes"("nom_equipe")
);
CREATE TABLE IF NOT EXISTS "Equipements" (
  "id_equipement" TEXT PRIMARY KEY,
  "type_id"       INTEGER NOT NULL,
  "nom"           TEXT,
  "code_parc"     TEXT,
  "numero_serie"  TEXT,
  "marque"        TEXT,
  "commentaire"   TEXT,
  "etat_pret"     INTEGER,
  "idrh"          INTEGER,
  FOREIGN KEY("type_id") REFERENCES "equipment_type"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  FOREIGN KEY("idrh")    REFERENCES "Agents"("idrh")
);
CREATE TABLE IF NOT EXISTS "Equipes" (
	"nom_equipe"	TEXT,
	"nom_site"	TEXT,
	PRIMARY KEY("nom_equipe"),
	FOREIGN KEY("nom_site") REFERENCES "Sites"("nom_site")
);
CREATE TABLE IF NOT EXISTS "Sites" (
	"nom_site"	TEXT,
	PRIMARY KEY("nom_site")
);
CREATE TABLE IF NOT EXISTS "Travail" (
	"idrh, nom_site"	TEXT,
	"nom_site"	TEXT,
	"idrh"	TEXT,
	PRIMARY KEY("idrh, nom_site"),
	FOREIGN KEY("idrh") REFERENCES "Agents"("idrh"),
	FOREIGN KEY("nom_site") REFERENCES "Sites"("nom_site")
);
CREATE TABLE IF NOT EXISTS equipment_type (
  id   INTEGER PRIMARY KEY AUTOINCREMENT,
  name TEXT NOT NULL UNIQUE
);
INSERT INTO "equipment_type" ("id","name") VALUES (1,'PC'),
 (2,'Écran'),
 (3,'Imprimante'),
 (4,'Routeur'),
 (5,'Switch'),
 (6,'Inconnu');
CREATE INDEX idx_equipements_type_id ON "Equipements"(type_id);
COMMIT;
