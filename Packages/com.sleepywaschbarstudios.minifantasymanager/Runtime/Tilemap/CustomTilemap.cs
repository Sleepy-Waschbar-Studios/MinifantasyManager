#nullable enable

/// <summary>
/// A rule tile descriptor is transformed into a series of actual cells.
/// 
/// The idea of descriptors is that you separate the "built" tilemap from the configuration of one.'
/// 
/// I do want to think about adding destructibles in the future but I'll likely have to autogenerate those
/// and it's unlikely a destructed object will do anything more than just remove the cells and replace them with a "destroyed" version.
/// </summary>
public class RuleTileDescriptor
{

}

/// <summary>
/// Unity has it's own tilemap which this is built ontop of (since as a single layer of sprites goes it's pretty well done).
/// 
/// We do the following;
/// 1. We build the world in a 3D environment so that we don't have to worry about sprite stacking and we get proper depth.
/// 2. We want custom rule tiles since a lot of our cells are multi-tile high (same with props).
/// 3. We have custom shadows to place down that have to be placed down relative to the cell which make it a bit tricky on a standard tilemap
/// 4. Height important tilemaps
/// 
/// The way we accomplish this is by building up the infrastructure ontop of the existing tilemaps + sprite workflow, that is;
/// 1. Tilemaps exist as simple layers that we stack ontop of each other, our "custom" tilemap may have many layers for example by default it will have;
///     - Pit
///     - Floor
///     - Wall/Doors
///     - Wall Shadows
/// 2. A single "cell" may take up any rectangular area i.e. a 1x1, 1x2/2x1, 2x2, ..., we don't currently support cutouts (corner pieces).
/// 3. Cells may have rules defined but those rules are encoded by our custom layer and written out to the tilemap.  Cells may react to **other** cells
///    for example, you could setup rule tiles that handle cute boundaries between two terrains.
///    
/// We want to have a runtime tilemap builder so it's semi-important that we have these abstractions because having to manually place above is
/// so annoying (and we could conceptually use custom rule tiles but it does get complicated quickly).
/// </summary>
public class CustomTilemap
{

}