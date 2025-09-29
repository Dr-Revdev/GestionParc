BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Agents" (
  "idrh"   TEXT PRIMARY KEY,
  "nom"    TEXT,
  "prenom" TEXT,
  "email"  TEXT,
  "equipe" TEXT,
  "site"   TEXT,          -- -> FK correcte
  "heberge" INTEGER,
  "commentaire" TEXT,
  FOREIGN KEY ("equipe") REFERENCES "Equipes"("nom_equipe"),
  FOREIGN KEY ("site")   REFERENCES "Sites"("nom_site")
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
  "idrh"     TEXT NOT NULL,
  "nom_site" TEXT NOT NULL,
  PRIMARY KEY ("idrh", "nom_site"),
  FOREIGN KEY ("idrh")     REFERENCES "Agents"("idrh"),
  FOREIGN KEY ("nom_site") REFERENCES "Sites"("nom_site")
);
CREATE TABLE IF NOT EXISTS equipment_type (
  id   INTEGER PRIMARY KEY AUTOINCREMENT,
  name TEXT NOT NULL UNIQUE
);
INSERT INTO "Agents" ("idrh","nom","prenom","email","equipe","site","heberge","commentaire") VALUES ('wsxd<<sc','wdvvv','wdvvvv','wdvvvvv','TNL','OLLIOULES',0,'sddsvsdv');
INSERT INTO "Equipements" ("id_equipement","type_id","nom","code_parc","numero_serie","marque","commentaire","etat_pret","idrh") VALUES ('9bdd1f4bdf924d5882529461934372b5',1,'UC833270TEST','ZY678','OP-TPT-PO','LENOVO','TEST',NULL,NULL),
 ('0382ee82186e447290ca9dee56191ec5',1,'UC833270TEST2','ZY123','ZDG-YHH-677','DELL','TEST2',NULL,NULL),
 ('180a41f3352748239520c01a5e4db217',1,'dfghjkl','xdcvbn,','dcfvgbhnj,k','xdcfgvbhnj','dfvgbhnj,k',NULL,NULL);
INSERT INTO "Equipes" ("nom_equipe","nom_site") VALUES ('VAR AZUR',NULL),
 ('COURRIER 13',NULL),
 ('MER & MONTAGNES',NULL),
 ('RMR',NULL),
 ('CODIR',NULL),
 ('RDP',NULL),
 ('TNL',NULL),
 ('CSO',NULL),
 ('HEBERGE',NULL),
 ('MONTCREC',NULL),
 ('AVICOR',NULL);
INSERT INTO "Sites" ("nom_site") VALUES ('MONTPELLIER'),
 ('NICE'),
 ('OLLIOULES');
INSERT INTO "Travail" ("idrh","nom_site") VALUES ('wsxd<<sc','OLLIOULES');
INSERT INTO "equipment_type" ("id","name") VALUES (1,'PC'),
 (2,'Écran'),
 (3,'Imprimante'),
 (4,'Routeur'),
 (5,'Switch'),
 (6,'Inconnu');
CREATE INDEX idx_equipements_type_id ON "Equipements"(type_id);
COMMIT;
