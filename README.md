# bbox-extractor
A program for extracting polygons from Microsoft's GlobalMLBuildingFootprints data, which are located inside a specified [bounding box](https://wiki.openstreetmap.org/wiki/Bounding_Box).

This data is in GeoJSONL format, which means every GeoJSON feature is on its own line. The program enumerates every line and checks if at least one of the coordinates of the polygon is inside a bounding box. In this case, the feature is written to the output file.

The output file is already in GeoJSON format to ensure maximum compatibility with programs that cannot parse GeoJSONL (yet).

Since the input files are pretty big (Germany has nearly 10 GiB), the files are not loaded into memory but parsed line by line. Running this program with the Germany extract processed over 28 million polygons and took about eight minutes on a nearly ten year old CPU with about 8 MiB RAM usage.

## Requirements
* .NET Core 3.1 runtime
* 64 bit Operating System

Tested on Debian 11 and Windows 7. Output files were successfully loaded in QGIS 3.26.

## Usage
    bbox-extractor Infile MinLon MinLat MaxLon MaxLat [Outfile]
    
    Infile          Path to source file in GeoJSONL format
    MinLon          Minimum longitude
    MinLat          Minimum latitude
    MaxLon          Maximum longitude
    MaxLat          Maximum latitude
    Outfile         Path to file with extracted polygons in GeoJOSN format
                    If not specified, the file will be saved in the
                    same directory as the Infile.

### Example
    bbox-extractor D:\Germany.geojsonl 10.2 50.1 11.9 51.6
Process the polygons located in ```D:\Germany.geojsonl``` and save every polygon inside the bounding box [10.2 50.1 11.9 51.6](https://tools.geofabrik.de/calc/#type=geofabrik_standard&bbox=10.2,50.1,11.9,51.6&tab=1&proj=EPSG:4326&places=1) in the file ```D:\Germany-extracted.geojson```.

### Workflow

1. [Download source file](https://github.com/microsoft/GlobalMLBuildingFootprints/blob/main/README.md#what-does-the-data-include) from Microsoft
2. Extract it
3. Choose your bounding box using [Geofabrik's excellent Tile Calculator](https://tools.geofabrik.de/calc/)
4. Run ```bbox-extractor``` with the source file and the chosen bounding box
5. If needed [convert the GeoJSON to OSM XML](https://github.com/tyrasd/geojsontoosm)
6. ...
7. Profit!

## Used libraries
* [CommunityToolkit](https://github.com/CommunityToolkit/dotnet) (© .NET Foundation and Contributors; MIT license)

## License
MIT License (see LICENSE)
