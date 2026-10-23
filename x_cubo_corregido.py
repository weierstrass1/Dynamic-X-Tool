"""
X-CUBO corregido. Blender 5.x recomendado.

Cambios importantes respecto del script original:
  * Usa el solver booleano MANIFOLD cuando Blender lo ofrece.
  * Comprueba que el resultado final sea una malla cerrada.
  * Suelda vertices coincidentes y elimina geometria degenerada.
  * Mantiene planas las caras principales para que no aparezcan diagonales
    oscuras producidas por smooth shading / Weighted Normal.
  * Conserva los colores por cara y las excepciones por zona.

Uso: abre este archivo en la pestana Scripting de Blender y pulsa Run Script.
"""

import colorsys
import math

import bmesh
import bpy


# ======================================================================
# CONFIGURACION
# ======================================================================

LADO = 2.0
GROSOR = 0.42
PUNTA = 0.55
BRAZOS_RECTOS = True  # True elimina el "codo" antes de cada punta
BISEL_ANCHO = 0.02
BISEL_SEGMENTOS = 3

COLORES = {
    "Frente": "#FF6B6B",
    "Atras": "#4ECDC4",
    "Derecha": "#FFD166",
    "Izquierda": "#7B6CF6",
    "Arriba": "#F1F1F5",
    "Abajo": "#2B2D42",
}

# Excepciones opcionales. Ejemplo:
# ZONAS = {("Frente", "arriba_izq"): "#2F80ED"}
ZONAS = {}

RUGOSIDAD = 0.30
METALICO = 0.0
MODO_DIAGNOSTICO = False
CREAR_ESCENA = True
NOMBRE = "XCubo"

CARAS = ["Frente", "Atras", "Derecha", "Izquierda", "Arriba", "Abajo"]
ZONAS_LATERALES = ["arriba_izq", "arriba_der", "abajo_izq", "abajo_der"]
ZONAS_VERTICALES = ["atras_izq", "atras_der", "frente_izq", "frente_der"]
DIRECCION = {
    "Frente": (0, -1, 0),
    "Atras": (0, 1, 0),
    "Derecha": (1, 0, 0),
    "Izquierda": (-1, 0, 0),
}


# ======================================================================
# FORMA Y CLASIFICACION
# ======================================================================

def contorno_x(grosor, punta):
    """Contorno antihorario de una X dentro de [-1, 1] x [-1, 1]."""
    grosor = min(max(grosor, 0.1), 0.9)
    punta = min(max(punta, 0.0), 0.9)
    medio = grosor / math.sqrt(2)
    radio = math.sqrt(2)

    # Contorno clasico de ocho vertices: muesca, punta, muesca, punta...
    # Cada costado de cada brazo es una sola recta, sin hombros ni quiebres.
    if BRAZOS_RECTOS:
        puntos = []
        for k in range(4):
            angulo = math.radians(45 + 90 * k)
            er = (math.cos(angulo), math.sin(angulo))
            es = (-math.sin(angulo), math.cos(angulo))

            def p(r, s):
                return (r * er[0] + s * es[0], r * er[1] + s * es[1])

            puntos.extend((p(medio, -medio), p(radio, 0.0)))
        return puntos

    # Forma antigua, disponible por compatibilidad. El punto "hombro" es el
    # que produce el codo visible antes de la punta.
    largo = radio - medio
    hombro = radio - punta * largo

    puntos = []
    for k in range(4):
        angulo = math.radians(45 + 90 * k)
        er = (math.cos(angulo), math.sin(angulo))
        es = (-math.sin(angulo), math.cos(angulo))

        def p(r, s):
            return (r * er[0] + s * es[0], r * er[1] + s * es[1])

        puntos.extend(
            (p(medio, -medio), p(hombro, -medio), p(radio, 0.0), p(hombro, medio))
        )
    return puntos


def punto_en_poligono(x, y, poligono):
    dentro = False
    j = len(poligono) - 1
    for i in range(len(poligono)):
        xi, yi = poligono[i]
        xj, yj = poligono[j]
        if (yi > y) != (yj > y) and x < (xj - xi) * (y - yi) / (yj - yi) + xi:
            dentro = not dentro
        j = i
    return dentro


def en_solido(punto, poligono):
    x, y, z = punto
    return punto_en_poligono(x, z, poligono) and punto_en_poligono(y, z, poligono)


def zonas_de(cara):
    return ZONAS_VERTICALES if cara in ("Arriba", "Abajo") else ZONAS_LATERALES


def cara_de(normal, centro):
    """Clasifica una superficie en una de las seis caras conceptuales."""
    nx, ny, nz = normal
    if abs(nx) < 1e-3 and abs(ny) < 1e-3:
        return "Arriba" if nz > 0 else "Abajo"

    if abs(nx) >= abs(ny):
        lateral = "Derecha" if nx > 0 else "Izquierda"
    else:
        lateral = "Atras" if ny > 0 else "Frente"

    dx, dy, dz = DIRECCION[lateral]
    profundidad = centro.x * dx + centro.y * dy + centro.z * dz
    if profundidad >= 0:
        return lateral
    return "Arriba" if nz > 0 else "Abajo"


def zona_de(cara, centro):
    cx, cy, cz = centro
    if cara in ("Arriba", "Abajo"):
        return ("frente" if cy < 0 else "atras") + "_" + (
            "izq" if cx < 0 else "der"
        )
    u = {"Frente": cx, "Atras": -cx, "Derecha": cy, "Izquierda": -cy}[cara]
    return ("arriba" if cz >= 0 else "abajo") + "_" + (
        "der" if u >= 0 else "izq"
    )


def clave_region(cara, zona):
    if MODO_DIAGNOSTICO or (cara, zona) in ZONAS:
        return (cara, zona)
    return (cara,)


# ======================================================================
# MATERIALES
# ======================================================================

def validar_configuracion():
    if LADO <= 0:
        raise ValueError("LADO debe ser mayor que cero.")
    if BISEL_ANCHO < 0 or BISEL_SEGMENTOS < 1:
        raise ValueError("El bisel necesita ancho >= 0 y segmentos >= 1.")
    for cara in CARAS:
        if cara not in COLORES:
            raise ValueError(f"Falta la cara '{cara}' en COLORES.")
    for clave in ZONAS:
        valida = (
            isinstance(clave, tuple)
            and len(clave) == 2
            and clave[0] in CARAS
            and clave[1] in zonas_de(clave[0])
        )
        if not valida:
            raise ValueError(f"Clave invalida en ZONAS: {clave!r}")


def paleta_diagnostico():
    combos = [(cara, zona) for cara in CARAS for zona in zonas_de(cara)]
    paleta = {}
    for i, combo in enumerate(combos):
        r, g, b = colorsys.hsv_to_rgb(i / len(combos), 0.75, 0.95)
        paleta[combo] = "#{:02X}{:02X}{:02X}".format(
            round(r * 255), round(g * 255), round(b * 255)
        )
    return paleta


def color_de(clave):
    if MODO_DIAGNOSTICO:
        return paleta_diagnostico()[clave]
    return ZONAS.get(clave) or COLORES[clave[0]]


def hex_a_rgba(hex_color):
    h = hex_color.lstrip("#")
    if len(h) != 6:
        raise ValueError(f"Color invalido: {hex_color!r}; usa '#RRGGBB'.")
    canales = [int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)]

    def lineal(c):
        return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4

    r, g, b = (lineal(c) for c in canales)
    return (r, g, b, 1.0)


def material(nombre, hex_color, rugosidad=None, metalico=None):
    mat = bpy.data.materials.get(nombre) or bpy.data.materials.new(nombre)
    mat.use_nodes = True
    color = hex_a_rgba(hex_color)

    arbol = mat.node_tree
    arbol.nodes.clear()
    salida = arbol.nodes.new("ShaderNodeOutputMaterial")
    bsdf = arbol.nodes.new("ShaderNodeBsdfPrincipled")
    salida.location = (300, 0)
    arbol.links.new(bsdf.outputs["BSDF"], salida.inputs["Surface"])
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Roughness"].default_value = (
        RUGOSIDAD if rugosidad is None else rugosidad
    )
    bsdf.inputs["Metallic"].default_value = METALICO if metalico is None else metalico
    mat.diffuse_color = color
    return mat


def nombre_material(clave):
    return f"{NOMBRE}_" + "_".join(clave)


def preparar_coleccion():
    col = bpy.data.collections.get(NOMBRE)
    if col is None:
        col = bpy.data.collections.new(NOMBRE)
        bpy.context.scene.collection.children.link(col)
        return col

    bancos = {
        "MESH": bpy.data.meshes,
        "CAMERA": bpy.data.cameras,
        "LIGHT": bpy.data.lights,
    }
    for obj in list(col.objects):
        datos = obj.data
        bpy.data.objects.remove(obj, do_unlink=True)
        if datos is not None and datos.users == 0:
            banco = bancos.get(datos.id_type)
            if banco is not None:
                banco.remove(datos)
    return col


# ======================================================================
# GEOMETRIA
# ======================================================================

def contorno_escalado():
    mitad = LADO / 2
    return [(a * mitad, b * mitad) for a, b in contorno_x(GROSOR, PUNTA)]


def malla_prisma(nombre, plano):
    """Prisma de X en XZ o YZ, cerrado y triangulado."""
    mitad = LADO / 2
    largo = mitad * 1.6
    contorno = contorno_x(GROSOR, PUNTA)

    def punto(a, b, s):
        if plano == "XZ":
            return (a * mitad, s, b * mitad)
        return (s, a * mitad, b * mitad)

    bm = bmesh.new()
    negativo = [bm.verts.new(punto(a, b, -largo)) for a, b in contorno]
    positivo = [bm.verts.new(punto(a, b, largo)) for a, b in contorno]
    bm.faces.new(negativo)
    bm.faces.new(positivo)
    for i in range(len(contorno)):
        j = (i + 1) % len(contorno)
        bm.faces.new((negativo[i], negativo[j], positivo[j], positivo[i]))

    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bmesh.ops.triangulate(bm, faces=list(bm.faces))
    malla = bpy.data.meshes.new(nombre)
    bm.to_mesh(malla)
    bm.free()
    return malla


def seleccionar_solver(modificador):
    """MANIFOLD corrige los huecos del solver EXACT vistos en Blender 5.x."""
    disponibles = set(modificador.bl_rna.properties["solver"].enum_items.keys())
    if "MANIFOLD" in disponibles:
        modificador.solver = "MANIFOLD"
        return "MANIFOLD"
    modificador.solver = "EXACT"
    return "EXACT"


def interseccion_de_prismas(col):
    malla_a = malla_prisma(f"{NOMBRE}_tmpA", "XZ")
    malla_b = malla_prisma(f"{NOMBRE}_tmpB", "YZ")
    obj_a = bpy.data.objects.new(f"{NOMBRE}_tmpA", malla_a)
    obj_b = bpy.data.objects.new(f"{NOMBRE}_tmpB", malla_b)
    col.objects.link(obj_a)
    col.objects.link(obj_b)

    mod = obj_a.modifiers.new("Interseccion", "BOOLEAN")
    mod.operation = "INTERSECT"
    mod.operand_type = "OBJECT"
    mod.object = obj_b
    solver = seleccionar_solver(mod)

    bpy.context.view_layer.update()
    dg = bpy.context.evaluated_depsgraph_get()
    resultado = bpy.data.meshes.new_from_object(
        obj_a.evaluated_get(dg), preserve_all_data_layers=False, depsgraph=dg
    )

    bpy.data.objects.remove(obj_a, do_unlink=True)
    bpy.data.objects.remove(obj_b, do_unlink=True)
    bpy.data.meshes.remove(malla_a)
    bpy.data.meshes.remove(malla_b)

    if not resultado.polygons:
        bpy.data.meshes.remove(resultado)
        raise RuntimeError("La interseccion booleana salio vacia.")
    return resultado, solver


def limpiar_bmesh(bm):
    tolerancia = max(LADO * 1e-7, 1e-9)
    bmesh.ops.remove_doubles(bm, verts=list(bm.verts), dist=tolerancia)
    bmesh.ops.dissolve_degenerate(bm, edges=list(bm.edges), dist=tolerancia)
    bm.normal_update()


def crear_solido(col):
    resultado, solver = interseccion_de_prismas(col)
    poligono = contorno_escalado()

    bm = bmesh.new()
    bm.from_mesh(resultado)
    limpiar_bmesh(bm)
    bmesh.ops.triangulate(bm, faces=list(bm.faces))
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.normal_update()

    # Corrige normales usando una prueba geometrica, no la orientacion heredada
    # de la booleana.
    eps = LADO * 1e-4
    volteadas = 0
    for cara in bm.faces:
        centro = cara.calc_center_median()
        normal = cara.normal
        muestra = (
            centro.x + normal.x * eps,
            centro.y + normal.y * eps,
            centro.z + normal.z * eps,
        )
        if en_solido(muestra, poligono):
            cara.normal_flip()
            volteadas += 1
    bm.normal_update()

    # Los cortes garantizan que una cara no cruce dos zonas de color.
    for normal_plano in ((1, 0, 0), (0, 1, 0), (0, 0, 1)):
        geometria = list(bm.verts) + list(bm.edges) + list(bm.faces)
        bmesh.ops.bisect_plane(
            bm,
            geom=geometria,
            dist=max(LADO * 1e-7, 1e-9),
            plane_co=(0.0, 0.0, 0.0),
            plane_no=normal_plano,
        )
    limpiar_bmesh(bm)
    bmesh.ops.triangulate(bm, faces=list(bm.faces))
    bm.normal_update()

    abiertas = sum(1 for arista in bm.edges if not arista.is_manifold)
    if abiertas:
        bm.free()
        bpy.data.meshes.remove(resultado)
        raise RuntimeError(
            f"El solver {solver} produjo {abiertas} aristas abiertas. "
            "Usa Blender 5.x, que ofrece el solver MANIFOLD."
        )

    numero_caras = len(bm.faces)
    bm.to_mesh(resultado)
    bm.free()
    resultado.name = f"{NOMBRE}_malla"

    # La booleana puede copiar slots vacios del operando. Si se conservan,
    # los indices calculados arriba apuntan a None y algunas caras salen grises.
    resultado.materials.clear()
    resultado.update()

    # Clasificar sobre la malla final es importante: al limpiar slots Blender
    # puede reajustar material_index, y las normales definitivas se calculan al
    # convertir el BMesh.
    claves = []
    leyenda = {}
    for poligono_malla in resultado.polygons:
        centro = poligono_malla.center
        nombre_cara = cara_de(poligono_malla.normal, centro)
        zona = zona_de(nombre_cara, centro)
        clave = clave_region(nombre_cara, zona)
        if clave not in claves:
            claves.append(clave)
            resultado.materials.append(
                material(nombre_material(clave), color_de(clave))
            )
        poligono_malla.material_index = claves.index(clave)
        poligono_malla.use_smooth = False
        leyenda[(nombre_cara, zona)] = clave

    obj = bpy.data.objects.new(NOMBRE, resultado)
    obj.location = (0.0, 0.0, LADO / 2)
    col.objects.link(obj)

    # Caras planas: evita triangulos oscuros y reflejos quebrados.
    resultado.update()

    if BISEL_ANCHO > 0:
        bisel = obj.modifiers.new("Bisel", "BEVEL")
        bisel.width = BISEL_ANCHO
        bisel.segments = BISEL_SEGMENTOS
        bisel.limit_method = "ANGLE"
        bisel.angle_limit = math.radians(25)

    escribir_leyenda(leyenda, numero_caras, volteadas, abiertas, solver)
    return obj


def escribir_leyenda(leyenda, numero_caras, volteadas, abiertas, solver):
    lineas = [
        "X-CUBO - regiones de color",
        "Para cambiar un color edita COLORES; para una zona usa ZONAS.",
        "",
        f"{'cara':<10} {'zona':<11} material (color)",
    ]
    for (cara, zona), clave in sorted(
        leyenda.items(), key=lambda item: (CARAS.index(item[0][0]), item[0][1])
    ):
        lineas.append(
            f"{cara:<10} {zona:<11} {nombre_material(clave)} ({color_de(clave)})"
        )
    lineas.extend(
        (
            "",
            f"Chequeo: solver {solver}, {numero_caras} triangulos, "
            f"{volteadas} normales corregidas, {abiertas} aristas abiertas.",
        )
    )
    texto = "\n".join(lineas)
    bloque = bpy.data.texts.get(f"{NOMBRE}_leyenda") or bpy.data.texts.new(
        f"{NOMBRE}_leyenda"
    )
    bloque.clear()
    bloque.write(texto)
    print(texto)


# ======================================================================
# ESCENA OPCIONAL
# ======================================================================

def crear_escena(col, objetivo):
    escala = LADO / 2

    camara_datos = bpy.data.cameras.new(f"{NOMBRE}_Camara")
    camara_datos.lens = 55
    camara = bpy.data.objects.new(f"{NOMBRE}_Camara", camara_datos)
    camara.location = (5.6 * escala, -7.0 * escala, 4.8 * escala)
    col.objects.link(camara)
    seguimiento = camara.constraints.new("TRACK_TO")
    seguimiento.target = objetivo
    seguimiento.track_axis = "TRACK_NEGATIVE_Z"
    seguimiento.up_axis = "UP_Y"
    bpy.context.scene.camera = camara

    def luz(nombre, posicion, energia, tamano_luz):
        datos = bpy.data.lights.new(nombre, "AREA")
        datos.energy = energia
        datos.size = tamano_luz
        objeto = bpy.data.objects.new(nombre, datos)
        objeto.location = tuple(valor * escala for valor in posicion)
        col.objects.link(objeto)
        seguir = objeto.constraints.new("TRACK_TO")
        seguir.target = objetivo
        seguir.track_axis = "TRACK_NEGATIVE_Z"
        seguir.up_axis = "UP_Y"

    luz(f"{NOMBRE}_LuzPrincipal", (4.5, -5.0, 6.0), 350, 4 * escala)
    luz(f"{NOMBRE}_LuzRelleno", (-6.0, -3.0, 3.0), 150, 5 * escala)
    luz(f"{NOMBRE}_LuzContra", (-2.0, 6.0, 5.0), 250, 4 * escala)

    if bpy.context.scene.world is None:
        mundo = bpy.data.worlds.new(f"{NOMBRE}_Mundo")
        bpy.context.scene.world = mundo
        mundo.use_nodes = True
        fondo = next(
            (nodo for nodo in mundo.node_tree.nodes if nodo.type == "BACKGROUND"),
            None,
        )
        if fondo is not None:
            fondo.inputs[0].default_value = (0.04, 0.04, 0.06, 1.0)


def main():
    validar_configuracion()
    activo = bpy.context.view_layer.objects.active
    if activo is not None and activo.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")

    coleccion = preparar_coleccion()
    objeto = crear_solido(coleccion)
    if CREAR_ESCENA:
        crear_escena(coleccion, objeto)

    bpy.ops.object.select_all(action="DESELECT")
    objeto.select_set(True)
    bpy.context.view_layer.objects.active = objeto
    print("XCubo listo. Ver diagnostico en el texto XCubo_leyenda.")


main()
