// 从 docs/hsk-resource-production-paths.md 提取 HSK 资源生产链路简表，
// 名称按当前 MOD（1.7/Defs，MMT_*）的中文 label 显示，未覆盖的用内置译名表，仍无则回退英文 label。
// 用法: node tools/build_resource_chains.js
// 输出: docs/hsk-resource-production-chains.md
'use strict';
const fs = require('fs');
const path = require('path');

const ROOT = path.resolve(__dirname, '..');
const SRC = path.join(ROOT, 'docs', 'hsk-resource-production-paths.md');
const DST = path.join(ROOT, 'docs', 'hsk-resource-production-chains.md');

// ---------- 内置译名表（HSK defName -> 中文） ----------
const ZH_RES = {
  // 矿石与散料
  Anglesite: '铅矿石', Aluminium: '铝土矿石', Uranium: '铀矿石', Ash: '灰烬', Dirt: '泥土',
  Compost: '堆肥', Fertilizer: '肥料', Peat: '泥炭', Nitre: '硝石', Salt: '盐',
  Sulphates: '硫酸盐', Paraffins: '石蜡', Surfactant: '表面活性剂燃料', Chemfuel: '化学燃料',
  OxidizedChemfuel: '氧化化学燃料', Napalm: '凝固汽油', Powder: '火药', Matter: '无机物质',
  Biomatter: '生物质', Antimatter: '反物质', FSX: 'FSX炸药',
  WoodLog: '原木', Bamboo: '竹原木', BambooPlank: '竹板', RedWoodLog: '红木原木', RedWoodPlank: '红木板',
  // 锭与合金
  Steel: '钢', SteelBar: '钢锭', AluminiumBar: '铝锭', LeadBar: '铅锭', GoldBar: '金锭',
  SilverBar: '纯银锭', CupronickelAlloy: '白铜合金', AlnicoAlloy: '铝镍钴合金',
  FerrosiliconAlloy: '硅铁合金', CarbonAlloy: '碳纤维合金', Plasteel: '铁钛合金',
  DepletedUranium: '贫铀合金', BioferriteAlloy: '生物铁素合金',
  // 零件与部件
  ComponentIndustrial: '工业零件', ComponentAdvanced: '高级零件', Weapon_Parts: '武器零件',
  Pistol_Component: '手枪零件', Rifle_Component: '步枪零件', AdvRifle_Component: '高级步枪零件',
  Sniper_Component: '狙击枪零件', AdvSniper_Component: '高级狙击枪零件', Shotgun_Component: '霰弹枪零件',
  SMG_Component: '冲锋枪零件', Heavy_Component: '重武器零件', Launcher_Component: '发射器零件',
  Cannon_Component: '火炮零件', Charged_Component: '充能武器零件', Laser_Component: '激光枪零件',
  Plasma_Component: '等离子武器零件', RobotParts: '机器人零件', MendingKit: '修补包',
  MetalCan: '金属罐', PaintingSupplies: '绘画用品', MineralSonarModule: '矿物声呐模块',
  BioMicrochips: '生物电子器件', MagneticMaterial: '磁性材料', Compaste: '复合膏',
  // 其他材料
  Ceramics: '陶瓷', ClayBrick: '粘土砖', Cloth: '布', DevilstrandCloth: '魔菇布',
  Hyperweave: '超织物', Synthread: '合成纤维织物', Flaxcloth: '亚麻布', HempCloth: '麻布',
  Velour: '天鹅绒', WoolSheep: '羊毛', Kevlar: '凯夫拉', Chitin: '几丁质',
  ChitinPlating: '几丁质护甲片', Bioferrite: '生物铁素', BiosyntheticMaterial: '生物合成材料',
  Jade: '翡翠', Coldstone: '冷石', Glowstone: '辉石', RawCaoutchouc: '天然生胶',
  Tallow: '动物油脂', Yeast: '酵母', ActivatedCarbon: '活性炭', Neutroamine: '中性胺',
  AlphaPoly: 'α聚合物', BetaPoly: 'β聚合物', Hexcell: 'HexCell电池',
};

const ZH_BLD = {
  ElectricSmelter: '电熔炉', TableFurnace: '手工锻炉', EAF: '电弧炉', OpenHearth: '平炉',
  Campfire: '篝火', Kiln: '窑炉', ElectricCrematorium: '电动焚化炉', CharcoalPit: '木炭窑坑',
  MatterConverter: '物质转换器', Matterfab: '物质制造机', CraftingSpot: '手工制作点',
  ButcherSpot: '屠宰点', TableButcher: '屠宰台', Brewery: '酿酒坊', ElectricBrewery: '电动酿酒坊',
  CheeseVat: '奶酪槽', CheeseBasin: '奶酪盆', CompostBarrel: '堆肥桶',
  ConcreteMixer: '混凝土搅拌机', EConcreteMixer: '电动混凝土搅拌机', DrugLab: '药物实验台',
  DryingRack: '晾干架', ElectricDryingRack: '电动晾干架', ElectricOven: '电烤箱',
  ElectricStove: '电炉灶', ElectricStove_Pro: '专业电炉灶', FueledStove: '燃料炉灶',
  TableGrill: '烧烤台', TableKitchen: '厨房台', TableOven: '烤箱台',
  ElectricSmithy: '电动锻造台', FueledSmithy: '燃料锻造台', ElectricTailoringBench: '电动裁缝台',
  HandTailoringBench: '手工裁缝台', FabricationBench: '装配制造台', GlassworkTable: '玻璃工坊',
  GrindStone: '磨刀石', RockCrusher: '碎石机', SK_Windmill: '风车磨坊', MillElectric: '电动磨坊',
  Saltpeter_pit: '硝石坑', SulfurExtractor: '硫磺提取器', TableChemlab: '化学实验台',
  TableElectronics: '电子工作台', TableMachining: '机械加工台', TableLoom: '织布机',
  TableCoffee: '咖啡桌', Canningstove: '罐头炉灶', CandyTable: '糖果桌', AmmoBench: '弹药工作台',
  AdvToolBench: '高级工具台', AdvWeaponCraftingWorkTable: '高级武器制造台',
  WeaponCraftingWorkTable: '武器制造台', MechWeaponCraftingWorkTable: '机械武器制造台',
  HeavyArmsBench: '重型武器台', TableHeavyAmmunition: '重型弹药台', TableOrganvat: '器官培养槽',
  LeatherworkerTable: '皮革工作台', SilagePile: '青贮饲料堆', JerkyRack: '肉干架',
  RobAssem: '机器人装配台', BioferriteShaper: '生物铁素塑形台', TablePrimitiveProsthetic: '原始假体台',
  TableRecycling: '回收台', TableSawmillElectric: '电动锯木台', TableSawmillHand: '手动锯木台',
  TableStonecutter: '切石台', UniversalFermenter: '万能发酵罐', VG_SaltMine: '盐矿场',
  meditable: '冥想桌', soylenttable: '合成食品台',
};

const ZH_CAT = {
  Metallic: '金属锭类', USLDBar: '超轻合金锭类', SLDBar: '轻合金锭类', HCMBar: '高碳金属锭类',
  HeavyBar: '重金属锭类', PRSBar: '耐压合金锭类', USLDHBar: '超硬合金锭类', RARBar: '稀有合金锭类',
  HCM: '高碳金属', MeatRaw: '生肉', MeatSubRaw: '替代生肉', FruitFoodRaw: '水果(生)',
  FungusPlantRaw: '菌类(生)', BasicPlantFoodRaw: '基础植物食材', ExtraPlantFoodRaw: '其他植物食材',
  PlantFoodRaw: '植物食材', StoneChunks: '石块', StoneBlocks: '石砖', Wooden: '木质',
  Woody: '木本植物', Coal: '煤类', BTextiles: '基础织物', Textiles: '织物', Leathers: '皮革',
  Corpses: '尸体', CorpsesHumanlike: '类人尸体', AnimalProductRaw: '动物产品(生)',
  EggsUnfertilized: '未受精蛋', EggsFertilized: '受精蛋', Chemical: '化学品',
  CookingSupplies: '烹饪辅料', BodyPartsNatural: '天然身体部位', WeaponsRanged: '远程武器',
  Waste: '废料', SyntheticOrgans: '合成器官', SimpleProstheses: '简易假体',
  BasicProstheses: '基础假体', BodyPartsUltra: '超级身体部位', BodyPartsBionic: '仿生部位',
  CyberneticProstheses: '控制论假体', ResourcesRaw: '原材料', OrganicOptimizer: '有机优化器',
  Foods: '食物', Fish: '鱼',
};

// MMT defName（去前缀） -> HSK defName 的别名；其余按自动规则匹配
const MMT_ALIAS = {
  Concrete: 'ConcreteResource', Sand: 'SandResource', ElectronicParts: 'ElectronicComponents',
  IntegratedCircuit: 'Electronics', Microchip: 'Microchips', SpacerComponent: 'ComponentSpacer',
  UltraComponent: 'ComponentUltra', HandFurnace: 'TableFurnace', ElectricArcFurnace: 'EAF',
  GlassworksTable: 'GlassworkTable', HandChemTable: 'TableChemlab', RoboticAssembler: 'RobAssem',
  HandSawmill: 'TableSawmillHand', ElectricSawmill: 'TableSawmillElectric',
  HandElectronicsTable: 'TableElectronics', ElectricElectronicsTable: 'TableElectronics',
};

// ---------- 读取 MMT 中文 label ----------
function walk(dir) {
  const out = [];
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) out.push(...walk(p));
    else if (e.name.endsWith('.xml')) out.push(p);
  }
  return out;
}
const mmtLabels = {};
for (const f of walk(path.join(ROOT, '1.7', 'Defs'))) {
  const t = fs.readFileSync(f, 'utf8');
  const re = /<defName>(MMT_[^<]+)<\/defName>\s*<label>([^<]+)<\/label>/g;
  let m;
  while ((m = re.exec(t))) mmtLabels[m[1]] = m[2];
}

// ---------- 解析详细版文档 ----------
const lines = fs.readFileSync(SRC, 'utf8').split(/\r?\n/);
let cat = null, res = null;
const entries = []; // {cat, resLabel, resDef, cells}
const resDefNames = new Set(), srcDefNames = new Set(), bldNames = new Set();
for (const line of lines) {
  let m = line.match(/^### \d+\.\d+ (.+?)\s*$/);
  if (m) { cat = m[1]; continue; }
  m = line.match(/^#### (.+?)（`([^`]+)`）/);
  if (m) { res = { label: m[1], def: m[2] }; resDefNames.add(m[2]); continue; }
  if (line.startsWith('| ') && res) {
    const cells = line.trim().replace(/^\||\|$/g, '').split('|').map(s => s.trim());
    if (cells.length < 9 || cells[0] === '途径' || /^[-: ]+$/.test(cells[0])) continue;
    const srcM = cells[1].match(/`([^`]+)`/);
    if (srcM) srcDefNames.add(srcM[1]);
    const bldM = cells[5].match(/建筑\s*[:：]\s*([^；]+)/);
    if (bldM) bldM[1].split(',').forEach(b => bldNames.add(b.trim()));
    entries.push({ cat, resLabel: res.label, resDef: res.def, cells });
  }
}

// ---------- 构建译名映射 ----------
const zhMap = Object.assign({}, ZH_RES, ZH_BLD);
const valid = new Set([...resDefNames, ...srcDefNames, ...bldNames]);
for (const [mDef, z] of Object.entries(mmtLabels)) {
  const x = mDef.slice(4);
  const cands = [MMT_ALIAS[x], x, x.endsWith('Ore') ? x.slice(0, -3) : null].filter(Boolean);
  for (const c of cands) {
    if (valid.has(c) && !zhMap[c]) { zhMap[c] = z; break; }
  }
}

function zh(defName, label) {
  const z = zhMap[defName];
  return z ? `${z}(${defName})` : `${label || defName}(${defName})`;
}
function trAlt(a) {
  let m = a.match(/^\[(.+)\]$/);
  if (m) return ZH_CAT[m[1]] ? `[${ZH_CAT[m[1]]}]` : a;
  m = a.match(/^([A-Za-z0-9_]+)（(.+)）$/);
  if (m) return zh(m[1], m[2]);
  return a;
}
function trItems(cell) {
  if (!cell || cell === '—') return '—';
  return cell.split('；').map(p => {
    p = p.trim();
    const qm = p.match(/^(.*?)\s*×\s*(.+)$/);
    const body = qm ? qm[1].trim() : p;
    const qty = qm ? '×' + qm[2].trim() : '';
    return body.split(' 或 ').map(a => trAlt(a.trim())).join(' 或 ') + qty;
  }).join(' + ');
}
function trSource(cell) {
  const m = cell.match(/^(.*?)（`([^`]+)`）/);
  return m ? zh(m[2], m[1]) : cell;
}
function trBld(s) {
  return s.split(',').map(b => {
    b = b.trim();
    const z = zhMap[b];
    return z ? `${z}(${b})` : b;
  }).join(', ');
}
function prereqOf(cell) {
  const out = {};
  if (!cell || cell === '—') return out;
  for (const part of cell.split('；')) {
    const m = part.trim().match(/^(研究|技能|建筑)\s*[:：]\s*(.+)$/);
    if (m) out[m[1]] = m[2].trim();
  }
  return out;
}
function kvOf(cell, keys) {
  if (!cell || cell === '—') return [];
  const found = [];
  for (const part of cell.split('；')) {
    const m = part.trim().match(/^([^=]+)=(.+)$/);
    if (m && keys.includes(m[1].trim())) found.push(`${m[1].trim()}=${m[2].trim()}`);
  }
  return found;
}
function defnameOf(cell) {
  const m = cell.match(/`([^`]+)`/);
  return m ? m[1] : cell;
}

// ---------- 生成链路行 ----------
const GATHER_TYPES = ['矿脉开采', '深钻井', '野外矿点', '摧毁残留', '植物收获',
  '动物挤奶', '动物剪毛', '动物产受精蛋', '动物产未受精蛋', '物质生成'];
const KV_KEYS = {
  工作台配方: ['工作量'], '发酵/缓处理配方': ['处理天数'],
  矿脉开采: ['矿团规模', '散布常见度'], 深钻井: ['每批规模', '常见度'],
  野外矿点: ['权重'], 植物收获: ['生长天数'],
};

const gather = new Map(GATHER_TYPES.map(t => [t, []]));
const recipes = new Map();  // 大类 -> Map(资源key -> {label, def, lines})
const ferment = new Map();

function pushNested(map, cat_, resLabel, resDef, line) {
  if (!map.has(cat_)) map.set(cat_, new Map());
  const rm = map.get(cat_);
  const key = resDef;
  if (!rm.has(key)) rm.set(key, { label: resLabel, def: resDef, lines: [] });
  rm.get(key).lines.push(line);
}

for (const { cat: cat_, resLabel, resDef, cells: c } of entries) {
  const [way, srcCell, ins, outs, kv, pre, note, cond] = c;
  const outTxt = trItems(outs);
  const condTxt = cond && cond !== '—' ? ` ｜ ${cond}` : '';
  const noteTxt = note && note !== '—' ? ` ｜ ${note}` : '';
  if (GATHER_TYPES.includes(way)) {
    let srcName;
    if (way === '深钻井') srcName = '深钻井';
    else if (way === '野外矿点') srcName = '野外矿点(WorkSite)';
    else srcName = trSource(srcCell);
    const kvTxt = kvOf(kv, KV_KEYS[way] || []);
    const kvS = kvTxt.length ? ` ｜ ${kvTxt.join('，')}` : '';
    gather.get(way).push(`- ${srcName} -> ${outTxt}${kvS}${noteTxt}${condTxt}`);
  } else if (way === '发酵/缓处理配方') {
    const rec = defnameOf(srcCell);
    const pre_ = prereqOf(pre);
    const kvTxt = kvOf(kv, ['处理天数']);
    const kvS = kvTxt.length ? ` ｜ ${kvTxt.join('，')}` : '';
    const bld = pre_['建筑'] ? ` @ ${trBld(pre_['建筑'])}` : '';
    pushNested(ferment, cat_, resLabel, resDef,
      `- ${trItems(ins)} -> ${outTxt}${bld}${kvS} ｜ \`${rec}\`${noteTxt}${condTxt}`);
  } else {
    const rec = defnameOf(srcCell);
    const pre_ = prereqOf(pre);
    const kvTxt = kvOf(kv, ['工作量']);
    const kvS = kvTxt.length ? ` ｜ ${kvTxt.join('，')}` : '';
    const bld = pre_['建筑'] ? ` @ ${trBld(pre_['建筑'])}` : '';
    const pq = [];
    if (pre_['研究']) pq.push(`研究:${pre_['研究']}`);
    if (pre_['技能']) pq.push(`技能:${pre_['技能']}`);
    const pqS = pq.length ? ` ｜ ${pq.join('，')}` : '';
    pushNested(recipes, cat_, resLabel, resDef,
      `- ${trItems(ins)} -> ${outTxt}${bld}${kvS}${pqS} ｜ \`${rec}\`${noteTxt}${condTxt}`);
  }
}

// ---------- 输出 ----------
const out = [];
out.push('# HSK 官方核心模块资源生产链路（简表）');
out.push('');
out.push('- 数据源：`Hardcore-SK` 分支 `1.7-super-unstable`，提交 `b99b17d2`；由 `tools/build_resource_chains.js` 从 [详细版](hsk-resource-production-paths.md) 自动生成。');
out.push('- 范围：`Core_SK` 与官方 `Core_SK_*` 模块。');
out.push('- 命名：`中文名(defName)`；中文名优先取当前 MOD（`1.7/Defs`，MMT_*）的中文 label，其次内置译名表，均未覆盖时显示英文 label。');
out.push('- 链路格式：`输入A×数量 + 输入B×数量 -> 产出×数量 @ 建筑 ｜ 关键数值 ｜ 前置 ｜ `配方defName` ｜ 备注/条件`。');
out.push('- 多产物配方会在每个产出资源小节下重复出现；`[木质]` 等方括号表示材料类别；`×按矿点规则` 表示数量由矿点营地规则决定。');
out.push('');

const nGather = [...gather.values()].reduce((s, v) => s + v.length, 0);
const countNested = m => [...m.values()].reduce((s, rm) => s + [...rm.values()].reduce((t, r) => t + r.lines.length, 0), 0);
const nRecipe = countNested(recipes), nFerment = countNested(ferment);

out.push('## 1. 汇总');
out.push('');
out.push('| 类别 | 链路数 |');
out.push('| --- | --- |');
out.push(`| 采集与自然产出 | ${nGather} |`);
out.push(`| 工作台配方 | ${nRecipe} |`);
out.push(`| 发酵/缓处理配方 | ${nFerment} |`);
out.push(`| **合计** | ${nGather + nRecipe + nFerment} |`);
out.push('');

let sec = 2;
out.push(`## ${sec}. 采集与自然产出`);
out.push('');
let i = 0;
for (const [way, rows] of gather) {
  if (!rows.length) continue;
  i++;
  out.push(`### ${sec}.${i} ${way}（${rows.length}）`);
  out.push('');
  out.push(...rows);
  out.push('');
}
sec++;

function emitNested(map, secNo, title) {
  out.push(`## ${secNo}. ${title}`);
  out.push('');
  let j = 0;
  for (const [cat_, rm] of map) {
    j++;
    const n = [...rm.values()].reduce((s, r) => s + r.lines.length, 0);
    out.push(`### ${secNo}.${j} ${cat_}（${n}）`);
    out.push('');
    for (const r of rm.values()) {
      out.push(`#### ${zh(r.def, r.label)}`);
      out.push('');
      out.push(...r.lines);
      out.push('');
    }
  }
}
emitNested(recipes, sec, '工作台配方（按产出资源分组）'); sec++;
emitNested(ferment, sec, '发酵/缓处理配方（按产出资源分组）');

fs.writeFileSync(DST, out.join('\n') + '\n', 'utf8');
const mapped = Object.keys(mmtLabels).length;
console.log(`${DST}`);
console.log(`解析 ${entries.length} 条 -> 采集 ${nGather} / 配方 ${nRecipe} / 发酵 ${nFerment}`);
console.log(`MMT 中文 label ${mapped} 个，译名表合计 ${Object.keys(zhMap).length} 项`);
