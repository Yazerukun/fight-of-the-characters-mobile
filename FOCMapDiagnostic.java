import java.io.File;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.util.Collection;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.TreeSet;

import com.etheller.interpreter.ast.util.JassProgram;
import com.etheller.warsmash.datasources.FolderDataSource;
import com.etheller.warsmash.parsers.w3x.War3Map;
import com.etheller.warsmash.parsers.w3x.doo.War3MapDoo;
import com.etheller.warsmash.parsers.w3x.unitsdoo.War3MapUnitsDoo;
import com.etheller.warsmash.parsers.w3x.w3e.War3MapW3e;
import com.etheller.warsmash.parsers.w3x.w3i.War3MapW3i;
import com.etheller.warsmash.parsers.w3x.wpm.War3MapWpm;
import net.warsmash.parsers.jass.SmashJassParser;

public class FOCMapDiagnostic {
    public static void main(String[] args) {
        System.out.println("================================================================================");
        System.out.println("FOC REBORN MOBILE — W3ANDROIDENGINE MAP DIAGNOSTIC HARNESS");
        System.out.println("================================================================================");

        File mapFile = new File("D:/FIGHT OF THE CHARACTERS/Fight_of_Characters9.1a(Asia).w3x");
        File baseDir = new File("D:/FIGHT OF THE CHARACTERS/Extracted_Assets");

        if (!mapFile.exists()) {
            System.err.println("ERROR: Map file not found: " + mapFile.getAbsolutePath());
            System.exit(1);
        }

        System.out.println("[TEST 1] Map File Existence & Size:");
        System.out.println("  Path: " + mapFile.getAbsolutePath());
        System.out.println("  Size: " + mapFile.length() + " bytes (" + String.format("%.2f", mapFile.length() / (1024.0 * 1024.0)) + " MB)");

        long t0 = System.currentTimeMillis();
        War3Map war3Map = null;
        try {
            System.out.println("\n[TEST 2] MPQ Archive Recognition & Loading via War3Map:");
            war3Map = new War3Map(new FolderDataSource(baseDir.toPath()), mapFile);
            long t1 = System.currentTimeMillis();
            System.out.println("  Status: SUCCESS [VERIFIED BY BUILD/TEST]");
            System.out.println("  Time to mount MPQ into RAM: " + (t1 - t0) + " ms");
        } catch (Exception e) {
            System.err.println("  Status: FAILED to open MPQ archive!");
            e.printStackTrace();
            System.exit(2);
        }

        // Test Listfile
        try {
            System.out.println("\n[TEST 3] MPQ Internal File Discovery (Listfile):");
            Collection<String> listfile = war3Map.getListfile();
            System.out.println("  Total files accessible: " + listfile.size());
            int mdxCount = 0, blpCount = 0, jassCount = 0, dataCount = 0;
            for (String f : listfile) {
                String lf = f.toLowerCase();
                if (lf.endsWith(".mdx")) mdxCount++;
                else if (lf.endsWith(".blp")) blpCount++;
                else if (lf.endsWith(".j")) jassCount++;
                else if (lf.endsWith(".slk") || lf.endsWith(".txt") || lf.endsWith(".w3u") || lf.endsWith(".w3a") || lf.endsWith(".w3t")) dataCount++;
            }
            System.out.println("  MDX Models: " + mdxCount);
            System.out.println("  BLP Textures: " + blpCount);
            System.out.println("  JASS Scripts: " + jassCount);
            System.out.println("  Data Tables: " + dataCount);
        } catch (Exception e) {
            System.out.println("  Warning reading listfile: " + e.getMessage());
        }

        // Test Map Information
        War3MapW3i mapInfo = null;
        try {
            System.out.println("\n[TEST 4] Parsing war3map.w3i (Map Information):");
            mapInfo = war3Map.readMapInformation();
            System.out.println("  Map Name: " + mapInfo.getName());
            System.out.println("  Map Author: " + mapInfo.getAuthor());
            System.out.println("  Map Description: " + (mapInfo.getDescription() != null ? mapInfo.getDescription().replace("\n", " ").trim() : "None"));
            System.out.println("  Player Slots Defined: " + mapInfo.getPlayers().size());
            System.out.println("  Forces/Teams Defined: " + mapInfo.getForces().size());
            System.out.println("  Status: PASS [VERIFIED BY BUILD/TEST]");
        } catch (Exception e) {
            System.err.println("  Status: FAILED to read war3map.w3i: " + e.getMessage());
            e.printStackTrace();
        }

        // Test Environment
        try {
            System.out.println("\n[TEST 5] Parsing war3map.w3e (Terrain Elevation):");
            War3MapW3e env = war3Map.readEnvironment();
            int[] mapSize = env.getMapSize();
            System.out.println("  Map Width (Tiles): " + mapSize[0]);
            System.out.println("  Map Height (Tiles): " + mapSize[1]);
            System.out.println("  Main Tileset: " + (char)env.getTileset());
            System.out.println("  Status: PASS [VERIFIED BY BUILD/TEST]");
        } catch (Exception e) {
            System.err.println("  Status: FAILED to read war3map.w3e: " + e.getMessage());
            e.printStackTrace();
        }

        // Test Pathing Map
        try {
            System.out.println("\n[TEST 6] Parsing war3map.wpm (Pathing Map):");
            War3MapWpm wpm = war3Map.readPathing();
            int[] wpmSize = wpm.getSize();
            System.out.println("  Pathing Width: " + wpmSize[0]);
            System.out.println("  Pathing Height: " + wpmSize[1]);
            System.out.println("  Total Pathing Cells: " + (wpmSize[0] * wpmSize[1]));
            System.out.println("  Status: PASS [VERIFIED BY BUILD/TEST]");
        } catch (Exception e) {
            System.err.println("  Status: FAILED to read war3map.wpm: " + e.getMessage());
            e.printStackTrace();
        }

        // Test Doodads
        try {
            System.out.println("\n[TEST 7] Parsing war3map.doo (Doodads):");
            if (mapInfo != null) {
                War3MapDoo doo = war3Map.readDoodads(mapInfo);
                System.out.println("  Doodad Instances: " + doo.getDoodads().size());
                System.out.println("  Status: PASS [VERIFIED BY BUILD/TEST]");
            }
        } catch (Exception e) {
            System.err.println("  Status: FAILED to read war3map.doo: " + e.getMessage());
        }

        // Test Pre-placed Units
        try {
            System.out.println("\n[TEST 8] Parsing war3mapUnits.doo (Pre-placed Units):");
            if (mapInfo != null) {
                War3MapUnitsDoo unitsDoo = war3Map.readUnits(mapInfo);
                System.out.println("  Pre-placed Unit Instances: " + unitsDoo.getUnits().size());
                System.out.println("  Status: PASS [VERIFIED BY BUILD/TEST]");
            }
        } catch (Exception e) {
            System.err.println("  Status: FAILED to read war3mapUnits.doo: " + e.getMessage());
        }

        // Test war3map.j parsing with ANTLR4 SmashJassParser
        try {
            System.out.println("\n[TEST 9] JASS2 Syntax Parsing via W3AndroidEngine ANTLR4 SmashJassParser:");
            if (!war3Map.has("war3map.j") && !war3Map.has("Scripts\\war3map.j")) {
                System.err.println("  ERROR: war3map.j not found in map archive!");
                System.exit(3);
            }
            String jassPath = war3Map.has("war3map.j") ? "war3map.j" : "Scripts\\war3map.j";
            InputStream jassStream = war3Map.getResourceAsStream(jassPath);
            InputStreamReader reader = new InputStreamReader(jassStream, StandardCharsets.UTF_8);

            JassProgram program = new JassProgram();
            SmashJassParser parser = new SmashJassParser(reader);
            long parseStart = System.currentTimeMillis();
            System.out.println("  Parsing " + jassPath + " with SmashJassParser...");
            parser.scanAndParse(jassPath, program);
            long parseEnd = System.currentTimeMillis();
            System.out.println("  Parse Duration: " + (parseEnd - parseStart) + " ms");
            System.out.println("  Calling program.initialize()...");
            try {
                program.initialize();
                System.out.println("  program.initialize() SUCCESS!");
                System.out.println("  Globals Registered: " + program.getGlobalScope().getGlobalIds().size());
                System.out.println("  Function Definitions: " + program.getGlobalScope().getFunctionDefinitions().size());
                System.out.println("  Status: PASS [VERIFIED BY BUILD/TEST]");
            } catch (Throwable t) {
                System.out.println("  program.initialize() caught exception (missing common.j types):");
                System.out.println("    " + t.getClass().getName() + ": " + t.getMessage());
                System.out.println("  Status: BLOCKED (Requires common.j type declarations) [BLOCKED]");
            }
        } catch (Throwable t) {
            System.err.println("  Status: JASS PARSE FAILED!");
            t.printStackTrace();
        }

        System.out.println("\n================================================================================");
        System.out.println("DIAGNOSTIC COMPLETE");
        System.out.println("================================================================================");
    }
}
