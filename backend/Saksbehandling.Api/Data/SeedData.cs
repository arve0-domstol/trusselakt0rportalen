using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Auth;
using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Data;

/// <summary>
/// Populates a fresh database with baseline reference and sample data:
/// departments, employee accounts, customers, cases and everything that
/// hangs off a case. Safe to call on every startup - it's a no-op once
/// data already exists.
/// </summary>
public static class SeedData
{
    private const string SharedPassword = "Trening2026!";

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var random = new Random(42);

        // ----- Departments -----
        var departments = new[]
        {
            new Department { Name = "Kundeservice", Code = "KS" },
            new Department { Name = "Økonomi", Code = "OKO" },
            new Department { Name = "Juridisk", Code = "JUR" },
            new Department { Name = "IT-drift", Code = "IT" },
            new Department { Name = "HR", Code = "HR" },
            new Department { Name = "Administrasjon", Code = "ADM" },
        };
        db.Departments.AddRange(departments);
        await db.SaveChangesAsync();

        var ks = departments[0];
        var oko = departments[1];
        var jur = departments[2];
        var it = departments[3];
        var hr = departments[4];
        var adm = departments[5];

        // ----- Users -----
        string PasswordHash() => PasswordHasher.Hash(SharedPassword);

        var users = new[]
        {
            new User { Username = "saksbehandler1", FullName = "Kari Andersen", Email = "kari.andersen@example-training.local", Phone = "40012301", Role = UserRole.Saksbehandler, DepartmentId = ks.Id, PasswordHash = PasswordHash() },
            new User { Username = "saksbehandler2", FullName = "Ola Nordmann", Email = "ola.nordmann@example-training.local", Phone = "40012302", Role = UserRole.Saksbehandler, DepartmentId = ks.Id, PasswordHash = PasswordHash() },
            new User { Username = "saksbehandler3", FullName = "Ingrid Haugen", Email = "ingrid.haugen@example-training.local", Phone = "40012303", Role = UserRole.SeniorSaksbehandler, DepartmentId = ks.Id, PasswordHash = PasswordHash() },
            new User { Username = "teamleder1", FullName = "Per Kristiansen", Email = "per.kristiansen@example-training.local", Phone = "40012304", Role = UserRole.Teamleder, DepartmentId = ks.Id, PasswordHash = PasswordHash() },
            new User { Username = "saksbehandler4", FullName = "Silje Berg", Email = "silje.berg@example-training.local", Phone = "40012305", Role = UserRole.Saksbehandler, DepartmentId = ks.Id, PasswordHash = PasswordHash() },
            new User { Username = "saksbehandler13", FullName = "Tor Arne Lie", Email = "tor.arne.lie@example-training.local", Phone = "40012306", Role = UserRole.Saksbehandler, DepartmentId = ks.Id, PasswordHash = PasswordHash(), IsActive = false },

            new User { Username = "saksbehandler5", FullName = "Anders Solberg", Email = "anders.solberg@example-training.local", Phone = "40012307", Role = UserRole.Saksbehandler, DepartmentId = oko.Id, PasswordHash = PasswordHash() },
            new User { Username = "saksbehandler6", FullName = "Marte Eide", Email = "marte.eide@example-training.local", Phone = "40012308", Role = UserRole.SeniorSaksbehandler, DepartmentId = oko.Id, PasswordHash = PasswordHash() },
            new User { Username = "teamleder2", FullName = "Thomas Lund", Email = "thomas.lund@example-training.local", Phone = "40012309", Role = UserRole.Teamleder, DepartmentId = oko.Id, PasswordHash = PasswordHash() },

            new User { Username = "saksbehandler7", FullName = "Hanne Iversen", Email = "hanne.iversen@example-training.local", Phone = "40012310", Role = UserRole.Saksbehandler, DepartmentId = jur.Id, PasswordHash = PasswordHash() },
            new User { Username = "saksbehandler8", FullName = "Erik Dahl", Email = "erik.dahl@example-training.local", Phone = "40012311", Role = UserRole.SeniorSaksbehandler, DepartmentId = jur.Id, PasswordHash = PasswordHash() },
            new User { Username = "saksbehandler14", FullName = "Maria Strand", Email = "maria.strand@example-training.local", Phone = "40012312", Role = UserRole.SeniorSaksbehandler, DepartmentId = jur.Id, PasswordHash = PasswordHash() },
            new User { Username = "teamleder3", FullName = "Kristin Moen", Email = "kristin.moen@example-training.local", Phone = "40012313", Role = UserRole.Teamleder, DepartmentId = jur.Id, PasswordHash = PasswordHash() },

            new User { Username = "saksbehandler9", FullName = "Vegard Halvorsen", Email = "vegard.halvorsen@example-training.local", Phone = "40012314", Role = UserRole.Saksbehandler, DepartmentId = it.Id, PasswordHash = PasswordHash() },
            new User { Username = "saksbehandler10", FullName = "Nina Vik", Email = "nina.vik@example-training.local", Phone = "40012315", Role = UserRole.Saksbehandler, DepartmentId = it.Id, PasswordHash = PasswordHash() },

            new User { Username = "saksbehandler11", FullName = "Håkon Fjeld", Email = "hakon.fjeld@example-training.local", Phone = "40012316", Role = UserRole.Saksbehandler, DepartmentId = hr.Id, PasswordHash = PasswordHash() },
            new User { Username = "saksbehandler12", FullName = "Camilla Skog", Email = "camilla.skog@example-training.local", Phone = "40012317", Role = UserRole.SeniorSaksbehandler, DepartmentId = hr.Id, PasswordHash = PasswordHash() },

            new User { Username = "administrator", FullName = "Admin Systemkonto", Email = "administrator@example-training.local", Phone = "40012318", Role = UserRole.Administrator, DepartmentId = adm.Id, PasswordHash = "40ed8c2f1c9f555d310d467e166f0c5ee78d10f88e97056b4ebb251a59f2281e" },
        };
        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        var usersByDept = users.Where(u => u.IsActive).GroupBy(u => u.DepartmentId).ToDictionary(g => g.Key, g => g.ToList());
        List<User> UsersIn(Department d) => usersByDept.TryGetValue(d.Id, out var list) ? list : new List<User>();

        // ----- Customers -----
        var customerHandles = new[]
        {
            "xXDarkWolf94Xx", "ShadowBroker_NO", "KryptoKongen", "NattravnOperator",
            "Ghost0perator", "ByteRaven77", "ZeroDayZelda", "ViperNode",
            "SilentPhish_NO", "CipherJackal", "BlackIceBandit", "RootkitRagnar",
            "PhantomPayload", "EclipseExploit", "MalwareMaiken", "TrojanTrygve",
            "WormwoodWilly", "BackdoorBjorn", "ExploitErling", "RansomRagna",
        };
        var companyNames = new[] { "Nordlys Krypto Konsult AS", "Baltic IT Solutions Ltd", "Fjordly Blockchain Services AS", "Solstad Offshore Hosting AS", "Lindebakke Digital Handel AS", "Vestlandet Data Trading AS" };
        var streets = new[] { "Storgata", "Kirkeveien", "Skolegata", "Fjellveien", "Strandgata", "Bjørkeveien", "Ringveien", "Parkveien", "Elvegata", "Industriveien" };
        var cityPostal = new (string City, string Postal)[]
        {
            ("Oslo", "0150"), ("Bergen", "5003"), ("Trondheim", "7010"), ("Stavanger", "4006"),
            ("Kristiansand", "4611"), ("Tromsø", "9008"), ("Drammen", "3044"), ("Fredrikstad", "1607"),
        };

        static string Slug(string s) => new(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

        var customers = new List<Customer>();
        for (var i = 0; i < 20; i++)
        {
            var (city, postal) = cityPostal[i % cityPostal.Length];
            var isCompany = i % 7 == 0;
            var name = isCompany ? companyNames[i / 7 % companyNames.Length] : customerHandles[i % customerHandles.Length];
            customers.Add(new Customer
            {
                Name = name,
                Address = $"{streets[i % streets.Length]} {random.Next(1, 120)}",
                PostalCode = postal,
                City = city,
                Phone = $"9{random.Next(1000000, 9999999):D7}",
                Email = isCompany
                    ? $"post@{Slug(name.Split(' ')[0])}.example-training.local"
                    : $"{Slug(name)}@kundetest-training.local",
                SyntheticNationalId = $"{random.Next(1, 28):D2}{random.Next(1, 12):D2}{random.Next(50, 99):D2}-{random.Next(10000, 99999)}",
            });
        }
        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();

        // ----- Cases -----
        // 58 unique templates - one per generated case (totalCases below), so the
        // 1:1 cycling produces no repeated titles/descriptions.
        var caseTemplates = new (string Title, string Description)[]
        {
            // A handful of ordinary, could-be-any-business tickets, kept for variety.
            ("Henvendelse om kontraktsvilkår", "Kunden har spørsmål om varighet og oppsigelsesfrist i inngått avtale, og ber om et skriftlig svar innen uken."),
            ("Forespørsel om innsyn", "Kunden ber om innsyn i egne registrerte opplysninger i forbindelse med en pågående henvendelse."),
            ("Endring av kontaktinformasjon", "Kunden ber om oppdatering av registrert adresse og telefonnummer etter en flytting."),
            ("Spørsmål om leveringstid", "Kunden lurer på forventet leveringstid for bestilt tjeneste og om det finnes mulighet for hasteleveranse."),
            ("Søknad om avdragsordning", "Kunden ønsker å inngå en nedbetalingsavtale og spør om rentevilkår."),
            ("Varsel om adresseendring", "Kunden har flyttet og varsler om ny adresse for fremtidig kommunikasjon."),

            // "Crime as a business" - internal operations, logistics, personnel.
            ("Spørsmål om provisjonsutbetaling", "Samarbeidspartner lurer på når neste utbetaling kommer, og truer med å gå til en konkurrerende gruppe hvis ikke pengene er på plass innen fredag."),
            ("Forespørsel om oppdatert prisliste", "Kunde ber om gjeldende priser for tjenestene som tilbys, og spør om det finnes mengderabatt ved kjøp av over ti angrep."),
            ("Ny rekrutt forsto ikke OPSEC-reglene", "Nyansatt affiliate la ut skjermbilder av et pågående angrep på eget Instagram, tagget med ekte navn. Saken håndteres nå av HR."),
            ("Uenighet om eierskap til stjålet data", "To interne team krangler om hvem som har retten til å selge en nylig stjålet kundedatabase videre."),
            ("Forespørsel om eksklusivitetsavtale", "Kunde ønsker enerett på tjenestene våre i et nytt marked, og tilbyr betaling i forskudd mot garantert kapasitet."),
            ("Klage på leveransekvalitet fra underleverandør", "Innleid utvikler leverte en bakdør med en kjent, allerede lappet sårbarhet. Kunden oppdaget dette selv og er ikke imponert."),
            ("Spørsmål om garantiordning", "Kunde spør om det finnes pengene-tilbake-garanti dersom et kjøpt verktøy blir oppdaget av antivirus innen 24 timer."),
            ("Internt varsel om mistenkelig oppførsel", "En affiliate stiller uvanlig mange spørsmål om interne rutiner og virker lite kjent med miljøet. HR ber om en vurdering."),

            // Attacks that went wrong.
            ("Løsepengevirus traff feil firma", "Partneren installerte krypteringsverktøyet på eget testmiljø ved en feil og krypterte dermed sin egen maskin, inkludert betalingsnøkkelen. Må nå betale løsepenger til seg selv for å få tilbake kontroll."),
            ("Phishing-kampanje stoppet av spamfilter", "Phishing-kampanjen ble fullstendig stoppet av offerets epost-filter. Kunden ønsker alle pengene i retur, han er rasende og truer med dårlig omtale på forumet."),
            ("DDoS-angrep rammet egen infrastruktur", "Botnettet ble feilkonfigurert og sendte all trafikk mot vår egen kontrollserver i stedet for målet. Serveren var nede i seks timer, og ingen fikk sendt kommandoer til noe som helst."),
            ("Feil offer ble kryptert", "Angrepet rammet et sykehjem i stedet for det tiltenkte regnskapsfirmaet, grunnet en forveksling av IP-adresser. Saken har fått stor medieoppmerksomhet, og kunden krever at alle spor fjernes umiddelbart."),
            ("Offer betalte med forfalsket krypto", "Løsepengebeløpet viste seg å være falske tokens uten reell verdi da det skulle veksles. Økonomiavdelingen mistenker at offeret visste nøyaktig hva de gjorde."),
            ("Ransomware-forhandling gikk i stå", "Offeret sluttet å svare på meldinger etter første kontakt og har koblet alt utstyr fra nettet. Ingen kontakt på ti dager, og dekrypteringsnøkkelen ligger ubrukt."),
            ("Mislykket SIM-swap mot VIP-mål", "Teleoperatøren krevde ekstra verifisering i skranken som vi ikke klarte å omgå per telefon. Oppdraget er satt på vent til en bedre metode er funnet."),
            ("Kryptominer oppdaget av antivirus", "Skjult miner på målets servere ble oppdaget og fjernet av standard antivirus etter bare tre dager. Inntektene dekket knapt strømregningen for operasjonen."),
            ("Falsk fakturasvindel avslørt av regnskapsfører", "En ellers overbevisende falsk faktura ble oppdaget fordi kontonummeret tilhørte et annet land enn den vanlige leverandøren. Målet varslet umiddelbart banken sin."),
            ("Deepfake-samtale ble gjennomskuet", "Den syntetiske stemmen til direktøren hakket akkurat idet offeret spurte om en detalj bare de to kjente til. Samtalen ble avsluttet brått og politianmeldt."),
            ("Watering hole-angrep traff feil nettsted", "Det infiserte nettstedet viste seg å ha null besøkende utenom oss selv. Tre uker med forberedelser resulterte i null infiserte mål."),
            ("Malvertising-kampanje blokkert av annonsenettverk", "Annonsene ble fjernet etter under én time på grunn av automatisk svindeldeteksjon. Reklamebudsjettet er dermed tapt uten en eneste visning som telte."),
            ("Smishing-kampanje endte som meme", "SMS-ene ble lagt ut som skjermdump på sosiale medier med teksten «dette er tydelig svindel» og spredte seg mer som vits enn som angrep."),
            ("USB-felle funnet av vaktmester", "USB-pinnene som ble plassert på parkeringsplassen ble funnet av vaktmesteren, som leverte dem pent inn til IT-avdelingen med en lapp om «mistenkelig gjenstand»."),
            ("Falsk kundestøtte-svindel gjennomskuet raskt", "Offeret ringte tilbake til det offisielle nummeret til selskapet før noe ble installert, og oppdaget raskt at samtalen var falsk."),
            ("Romantikksvindel avslørt av offerets datter", "Offerets voksne datter gjenkjente profilbildet fra en kjent svindelsamling og stoppet overføringen i siste liten."),
            ("Exploit-kit virket ikke på oppdatert nettleser", "Sårbarheten exploit-kitet baserte seg på ble lappet to uker tidligere. Ingen av testbesøkene resulterte i noe som helst."),
            ("Nulldagssårbarhet solgt til feil kjøper", "Sårbarheten ble solgt videre til en kjøper som viste seg å jobbe for et sikkerhetsselskap under dekknavn. Detaljene er nå offentlig kjent."),
            ("Dataeksfiltrering stoppet av nettverksovervåking", "Den store filoverføringen ut av nettverket ble flagget av målets overvåkingssystem etter bare noen minutter, lenge før noe nyttig rakk å komme ut."),
            ("Dobbel utpressing uten kryptering slo feil", "Trusselen om å publisere stjålne data ble ignorert av offeret, som allerede hadde varslet alle berørte selv. Utpressingen mistet all effekt."),
            ("Falsk programvareoppdatering avvist av IT-avdeling", "Den ondsinnede oppdateringen ble fanget opp i en rutinemessig gjennomgang før den rakk å nå en eneste ansatt-PC."),
            ("Evil twin-nettverk oppdaget av årvåken gjest", "En gjest på kaffebaren la merke til to identiske wifi-nettverk og varslet personalet før noen rakk å koble seg til det falske."),
            ("Ondsinnet nettleserutvidelse fjernet av nettbutikken", "Utvidelsen ble fjernet fra nettbutikken for utvidelser etter under 48 timer, lenge før den nådde det planlagte antallet installasjoner."),
            ("Typosquatting-domene krevd stengt", "Det forfalskede domenet ble rapportert og stengt av registraren samme dag det ble satt i drift. Investeringen i domenenavnet er tapt."),
            ("Falskt jobbtilbud gjennomskuet av erfaren utvikler", "Målet googlet selskapet som angivelig tilbød jobben og fant ingen treff utenom en tom nettside opprettet samme uke."),
            ("Tilgang solgt videre viste seg å være ubrukelig", "Tilgangen som ble solgt til en annen gruppe sluttet å virke timer etter kjøpet, antagelig fordi offeret allerede hadde byttet passord av andre årsaker."),
            ("Phishing-side avslørt av feil i norsk språk", "Den forfalskede innloggingssiden brukte Google-oversatt tekst med åpenbare feil, og flere mål rapporterte den til banken i stedet for å logge inn."),
            ("Falsk konkurransevarsel ga null respons", "SMS-en om en vunnet premie ble sendt til 10 000 numre, men ingen klikket. Trolig fordi «premien» var et produkt ingen av mottakerne kjente til."),
            ("QR-kode-svindel skannet av feil person", "QR-koden limt over den ekte betalingsautomaten ble skannet av en nysgjerrig sikkerhetsvakt, som varslet eieren av stedet umiddelbart."),
            ("Falsk app fjernet fra app-butikken på timer", "Den ondsinnede appen ble fjernet av app-butikkens automatiske kontroll før den rakk mer enn et par hundre nedlastinger, stort sett fra eget team under testing."),

            // The threat actors themselves getting hacked.
            ("C2-serveren vår er kompromittert", "Ukjent part har fått tilgang til kommando-og-kontroll-serveren via et standardpassord som aldri ble endret. Alle offerlogger kan være eksponert."),
            ("Betalingslommeboken er tømt av ukjent aktør", "Hele beholdningen ble overført til en ukjent lommebok midt på natten. Transaksjonen kan ikke reverseres, og ingen vet hvem som satt med nøklene."),
            ("Konkurrerende gruppe har lekket kildekoden vår", "Verktøyet vårt dukket opp på et offentlig forum sammen med interne notater og en hånlig kommentar om kodekvaliteten."),
            ("Politiet har beslaglagt en av serverne våre", "En utenlandsk hostingleverandør mistet en server til et internasjonalt politisamarbeid. Juridisk ber om en umiddelbar risikovurdering av hva som lå lagret der."),
            ("Noen har hacket chat-serveren vår", "Interne samtaler mellom medlemmer er nå tilgjengelig på et sikkerhetsforskningsnettsted, inkludert en lang diskusjon om hvem som glemte å betale for VPN-abonnementet."),
            ("Mistenker infiltrasjon fra sikkerhetsforsker", "En bruker med tilgang til forumet stiller spørsmål som er påfallende presise for noen som angivelig bare er nybegynner. Flere medlemmer anbefaler å kutte kontakten."),
            ("Egen phishing-side ble selv phishet", "Innloggingssiden vi bruker for å stjele legitimasjon har blitt kopiert og manipulert av en tredjepart til å stjele våre egne eiendeler i stedet."),
            ("Affiliate-panelet er nede etter motangrep", "Et ukjent sikkerhetsselskap ser ut til å ha utnyttet en sårbarhet i vårt eget affiliate-panel og slått det av fullstendig, med en melding om at saken er varslet til politiet."),
            ("Egen ransomware-bygger inneholdt bakdør", "Verktøyet vi kjøpte for å bygge krypteringsprogrammer viste seg å sende en kopi av alle nøkler til utvikleren. Alle tidligere ofre må nå regnes som potensielt dekryptert av andre."),
            ("Rivaliserende gruppe har doxxet en av våre operatører", "Virkelig navn, adresse og bilder av en sentral operatør er lagt ut offentlig av en konkurrerende gruppe etter en krangel om territorium."),
            ("Forum-kontoen vår er overtatt", "Hovedkontoen vår på det interne forumet er kapret, og noen later nå til å være oss og selger falske tjenester i vårt navn."),
            ("Krypteringsnøklene våre er offentliggjort", "Noen har publisert alle dekrypteringsnøkler for våre siste 200 ofre på et offentlig nettsted, uten at vi aner hvem eller hvorfor."),
            ("Honningfelle avslørte en av medlemmene våre", "Et av medlemmene våre har i flere måneder kommunisert med det som viste seg å være en skuespiller ansatt av et sikkerhetsselskap. Mye intern informasjon kan være kompromittert."),
            ("Egen VPN-leverandør loggførte alt likevel", "Tjenesten vi stolte på for å være loggfri, viste seg å ha overvåket alt og samarbeider nå med myndigheter i flere land."),
        };

        var statusPool = new[]
        {
            CaseStatus.Ny, CaseStatus.Ny,
            CaseStatus.UnderBehandling, CaseStatus.UnderBehandling, CaseStatus.UnderBehandling,
            CaseStatus.VenterPaaKunde,
            CaseStatus.TilGodkjenning,
            CaseStatus.Godkjent,
            CaseStatus.Lukket, CaseStatus.Lukket,
            CaseStatus.Avvist,
        };
        var priorityPool = new[]
        {
            CasePriority.Lav,
            CasePriority.Normal, CasePriority.Normal, CasePriority.Normal, CasePriority.Normal,
            CasePriority.Hoy, CasePriority.Hoy,
            CasePriority.Kritisk,
        };

        var allDepartments = departments;
        var cases = new List<Case>();
        var caseSeq = 1;
        var hrCaseIdSeq = 10000;
        const int totalCases = 58;

        for (var i = 0; i < totalCases; i++)
        {
            var dept = allDepartments[i % (allDepartments.Length - 1)]; // exclude Administrasjon from case ownership
            var template = caseTemplates[i % caseTemplates.Length];
            var customer = customers[i % customers.Count];
            var status = statusPool[random.Next(statusPool.Length)];
            var priority = priorityPool[random.Next(priorityPool.Length)];
            var createdAt = DateTime.UtcNow.AddDays(-random.Next(5, 120));
            var updatedAt = createdAt.AddDays(random.Next(0, 10));

            var deptUsers = UsersIn(dept);
            User? assignedTo = status == CaseStatus.Ny
                ? null
                : deptUsers.Count > 0 ? deptUsers[random.Next(deptUsers.Count)] : null;

            var c = new Case
            {
                Id = dept == hr ? hrCaseIdSeq++ : 0,
                CaseNumber = $"SAK-2026-{caseSeq:D5}",
                Title = template.Title,
                Description = template.Description,
                CustomerId = customer.Id,
                DepartmentId = dept.Id,
                AssignedToUserId = assignedTo?.Id,
                Status = status,
                Priority = priority,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                Deadline = createdAt.AddDays(random.Next(7, 45)),
            };

            if (status is CaseStatus.Lukket or CaseStatus.Godkjent)
            {
                c.ClosedAt = updatedAt;
                var approverCandidates = deptUsers.Where(u => u.Role >= UserRole.SeniorSaksbehandler).ToList();
                c.ApprovedByUserId = approverCandidates.Count > 0
                    ? approverCandidates[random.Next(approverCandidates.Count)].Id
                    : assignedTo?.Id;
            }

            cases.Add(c);
            caseSeq++;
        }

        // A handful of specific, deterministic cases layered on top of the
        // generated pool, useful as fixed reference points once the API and
        // frontend exist (stable IDs/case numbers to link to from later
        // documentation and tests).
        var kariUser = users.First(u => u.Username == "saksbehandler1");
        var ninaUser = users.First(u => u.Username == "saksbehandler10");
        var torArneUser = users.First(u => u.Username == "saksbehandler13");

        cases.Add(new Case
        {
            CaseNumber = $"SAK-2026-{caseSeq++:D5}",
            Title = "Løsepengebetaling forsvant - hastesak",
            Description = "Oppdragsgiver hevder løsepengebeløpet på 2,3 bitcoin aldri kom frem til riktig lommebok, og mistenker at en affiliate har stukket av med pengene selv. Kunden ringer daglig og krever umiddelbar avklaring før han eskalerer til «andre kanaler».",
            CustomerId = customers[0].Id,
            DepartmentId = ks.Id,
            AssignedToUserId = ninaUser.Id,
            Status = CaseStatus.UnderBehandling,
            Priority = CasePriority.Hoy,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-2),
            Deadline = DateTime.UtcNow.AddDays(-3), // already past its deadline
        });

        cases.Add(new Case
        {
            CaseNumber = $"SAK-2026-{caseSeq++:D5}",
            Title = "Mislykket phishing-kampanje aldri fulgt opp",
            Description = "Phishing-kampanjen ble fullstendig stoppet av offerets epost-filter, og ingen av de 500 utsendte e-postene nådde frem. Kunden ønsker alle pengene i retur og er rasende, men saken har stått urørt siden saksbehandleren sluttet.",
            CustomerId = customers[1].Id,
            DepartmentId = ks.Id,
            AssignedToUserId = torArneUser.Id,
            Status = CaseStatus.UnderBehandling,
            Priority = CasePriority.Normal,
            CreatedAt = DateTime.UtcNow.AddDays(-60),
            UpdatedAt = DateTime.UtcNow.AddDays(-45),
            Deadline = DateTime.UtcNow.AddDays(10),
        });

        db.Cases.AddRange(cases);
        await db.SaveChangesAsync();

        // ----- Case assignment history -----
        var assignments = new List<CaseAssignment>();
        foreach (var c in cases.Where(c => c.AssignedToUserId.HasValue))
        {
            var deptUsers = UsersIn(allDepartments.First(d => d.Id == c.DepartmentId));
            var assigner = deptUsers.FirstOrDefault(u => u.Role >= UserRole.Teamleder) ?? kariUser;
            assignments.Add(new CaseAssignment
            {
                CaseId = c.Id,
                AssignedToUserId = c.AssignedToUserId!.Value,
                AssignedByUserId = assigner.Id,
                AssignedAt = c.CreatedAt.AddHours(random.Next(1, 48)),
                Note = "Fordelt ved opprettelse.",
            });
        }
        db.CaseAssignments.AddRange(assignments);

        // ----- Comments -----
        var commentTemplates = new (string Text, bool Internal)[]
        {
            ("Har kontaktet oppdragsgiver for å avklare detaljer.", false),
            ("Venter på tilbakemelding fra kontakten.", false),
            ("Mottatt etterspurt dokumentasjon.", false),
            ("Saken er komplisert - bør vurderes av senior saksbehandler.", true),
            ("Oppdragsgiver virker opprørt over mislykket leveranse, følg opp forsiktig.", true),
            ("Dobbeltsjekket historikk - ingen tidligere klager fra denne kontakten.", true),
            ("Sendt oppdatert status til kontakten per kryptert melding.", false),
            ("Avklart med avdelingsleder hvordan saken skal håndteres - holder lav profil.", true),
            ("Oppdragsgiver har akseptert forslag til løsning.", false),
            ("Trenger mer informasjon før saken kan avgjøres - mulig infiltrasjon, vær forsiktig.", true),
        };

        var comments = new List<Comment>();
        foreach (var c in cases)
        {
            var commentCount = random.Next(0, 4);
            var deptUsers = UsersIn(allDepartments.First(d => d.Id == c.DepartmentId));
            var possibleAuthors = deptUsers.Count > 0 ? deptUsers : new List<User> { kariUser };
            for (var j = 0; j < commentCount; j++)
            {
                var template = commentTemplates[random.Next(commentTemplates.Length)];
                comments.Add(new Comment
                {
                    CaseId = c.Id,
                    AuthorUserId = possibleAuthors[random.Next(possibleAuthors.Count)].Id,
                    Text = template.Text,
                    IsInternal = template.Internal,
                    CreatedAt = c.CreatedAt.AddDays(random.Next(0, 15)),
                });
            }
        }
        db.Comments.AddRange(comments);

        // ----- Documents -----
        var fileTemplates = new (string FileName, string ContentType)[]
        {
            ("dokumentasjon.pdf", "application/pdf"),
            ("kundehenvendelse.pdf", "application/pdf"),
            ("vedtak.pdf", "application/pdf"),
            ("faktura.pdf", "application/pdf"),
            ("kontrakt.pdf", "application/pdf"),
            ("intern-notat.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
            ("skjermbilde.png", "image/png"),
            ("tilbud.pdf", "application/pdf"),
        };

        var documents = new List<Document>();
        var casesForDocuments = cases.Where((_, idx) => idx % 3 != 0).Take(42);
        foreach (var c in casesForDocuments)
        {
            var template = fileTemplates[random.Next(fileTemplates.Length)];
            var deptUsers = UsersIn(allDepartments.First(d => d.Id == c.DepartmentId));
            var uploader = deptUsers.Count > 0 ? deptUsers[random.Next(deptUsers.Count)] : kariUser;
            var storedFileName = $"{Guid.NewGuid():N}_{template.FileName}";
            documents.Add(new Document
            {
                CaseId = c.Id,
                FileName = template.FileName,
                StoredFileName = storedFileName,
                ContentType = template.ContentType,
                SizeBytes = random.Next(15_000, 2_500_000),
                UploadedByUserId = uploader.Id,
                UploadedAt = c.CreatedAt.AddDays(random.Next(0, 10)),
                StoragePath = $"cases/{c.Id}/{storedFileName}",
            });
        }
        db.Documents.AddRange(documents);

        // ----- Audit events -----
        var auditEvents = new List<AuditEvent>();
        foreach (var c in cases)
        {
            auditEvents.Add(new AuditEvent
            {
                EntityType = nameof(Case),
                EntityId = c.Id,
                UserId = c.AssignedToUserId,
                Action = "Opprettet",
                Details = $"Sak {c.CaseNumber} opprettet.",
                Timestamp = c.CreatedAt,
            });
            if (c.AssignedToUserId.HasValue)
            {
                auditEvents.Add(new AuditEvent
                {
                    EntityType = nameof(Case),
                    EntityId = c.Id,
                    UserId = c.AssignedToUserId,
                    Action = "Tildelt",
                    Details = "Sak tildelt saksbehandler.",
                    Timestamp = c.CreatedAt.AddHours(random.Next(1, 48)),
                });
            }
            if (c.Status != CaseStatus.Ny)
            {
                auditEvents.Add(new AuditEvent
                {
                    EntityType = nameof(Case),
                    EntityId = c.Id,
                    UserId = c.AssignedToUserId,
                    Action = "StatusEndret",
                    Details = $"Status endret til {c.Status}.",
                    Timestamp = c.UpdatedAt,
                });
            }
        }
        db.AuditEvents.AddRange(auditEvents);

        // ----- Notifications -----
        var notifications = new List<Notification>();
        foreach (var c in cases.Where(c => c.AssignedToUserId.HasValue).Take(35))
        {
            notifications.Add(new Notification
            {
                UserId = c.AssignedToUserId!.Value,
                Message = $"Ny sak tildelt deg: {c.CaseNumber} - {c.Title}",
                IsRead = random.Next(0, 2) == 0,
                CreatedAt = c.CreatedAt.AddHours(2),
                RelatedCaseId = c.Id,
            });
        }
        foreach (var c in cases.Where(c => c.Deadline < DateTime.UtcNow.AddDays(3) && c.Status != CaseStatus.Lukket && c.AssignedToUserId.HasValue).Take(10))
        {
            notifications.Add(new Notification
            {
                UserId = c.AssignedToUserId!.Value,
                Message = $"Frist nærmer seg for sak {c.CaseNumber}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                RelatedCaseId = c.Id,
            });
        }
        db.Notifications.AddRange(notifications);

        await db.SaveChangesAsync();
    }
}
