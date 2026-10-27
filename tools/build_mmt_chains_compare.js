// 生成当前 MOD（MMT）资源生产链路，并与 HSK 官方链路逐条对比。
// 用法: node tools/build_mmt_chains_compare.js
// 输出: docs/mmt-vs-hsk-resource-chains.md
'use strict';
const fs = require('fs');
const path = require('path');

const ROOT = path.resolve(__dirname, '..');
const HSK_DOC = path.join(ROOT, 'docs', 'hsk-resource-production-paths.md');
const DST = path.join(ROOT, 'docs', 'mmt-vs-hsk-resource-chains.md');

// ---------- MMT -> HSK defName 别名（去 MMT_ 前缀后不一致的），与 tools/build_mmt_*_doc.py 共用 ----------
const MMT_ALIAS = JSON.parse(fs.readFileSync(path.join(ROOT, 'tools', 'data', 'mmt_hsk_alias.json'), 'utf8'));
delete MMT_ALIAS._comment;

// ---------- 读取 MMT Defs ----------
function walk(dir) {
  const out = [];
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) out.push(...walk(p));
    else if (e.name.endsWith('.xml')) out.push(p);
  }
  return out;
}
function grab(block, tag) {
  const m = block.match(new RegExp(`<${tag}>([^<]+)</${tag}>`));
  return m ? m[1].trim() : null;
}
function grabBlock(block, tag) {
  const m = block.match(new RegExp(`<${tag}>([\\s\\S]*?)</${tag}>`));
  return m ? m[1] : null;
}
function liList(block) {
  if (!block) return [];
  return [...block.matchAll(/<li>([^<]+)<\/li>/g)].map(m => m[1].trim());
}
function kvMap(block) {
  const out = {};
  if (!block) return out;
  for (const m of block.matchAll(/<([A-Za-z0-9_.]+)>([^<]+)<\/[A-Za-z0-9_.]+>/g)) out[m[1]] = m[2].trim();
  return out;
}

const defsDir = path.join(ROOT, '1.7', 'Defs');
const labels = {};    // 所有 MMT defName -> 中文 label
const recipes = [];   // MMT 配方
const buildings = {}; // defName -> {label, recipes[], research[], mineable}
const research = {};  // defName -> label

for (const f of walk(defsDir)) {
  const t = fs.readFileSync(f, 'utf8');
  const family = path.basename(f).replace(/^MMT_(Recipes|Buildings)_/, '').replace(/\.xml$/, '');
  for (const m of t.matchAll(/<(RecipeDef|ThingDef|ResearchProjectDef)\b[^>]*>([\s\S]*?)<\/\1>/g)) {
    const kind = m[1], b = m[2];
    const def = grab(b, 'defName');
    if (!def) continue;
    const label = grab(b, 'label');
    if (label) labels[def] = label;
    if (kind === 'RecipeDef') {
      const ings = [];
      const ingBlock = grabBlock(b, 'ingredients');
      // 配料 <li> 内部还有嵌套 <li>（thingDefs），须按 <filter>..</filter><count>.. 结构整体匹配
      for (const li of ingBlock ? ingBlock.matchAll(/<li>\s*<filter>([\s\S]*?)<\/filter>\s*<count>([^<]+)<\/count>\s*<\/li>/g) : []) {
        const filter = li[1];
        const defs = liList(grabBlock(filter, 'thingDefs'));
        const cats = liList(grabBlock(filter, 'categories'));
        const count = parseFloat(li[2]);
        ings.push({ defs, cats, count });
      }
      const prods = [];
      const prodBlock = grabBlock(b, 'products');
      if (prodBlock) {
        for (const pm of prodBlock.matchAll(/<([A-Za-z0-9_]+)>([^<]+)<\/[A-Za-z0-9_]+>/g)) {
          prods.push({ def: pm[1], qty: parseFloat(pm[2]) });
        }
      }
      const skills = [];
      const skBlock = grabBlock(b, 'skillRequirements');
      if (skBlock) {
        for (const sm of skBlock.matchAll(/<([A-Za-z0-9_]+)>([^<]+)<\/[A-Za-z0-9_]+>/g)) {
          skills.push(`${sm[1]}≥${sm[2].trim()}`);
        }
      }
      recipes.push({
        def, label, family,
        work: parseInt(grab(b, 'workAmount') || '0', 10),
        research: grab(b, 'researchPrerequisite'),
        skills, ings, prods,
      });
    } else if (kind === 'ThingDef') {
      const bld = grabBlock(b, 'building');
      const mineThing = bld ? grab(bld, 'mineableThing') : null;
      const statBases = kvMap(grabBlock(b, 'statBases'));
      buildings[def] = {
        label, file: path.basename(f), dir: path.basename(path.dirname(f)),
        size: grab(b, 'size'),
        costStuffCount: grab(b, 'costStuffCount'),
        stuffCats: liList(grabBlock(b, 'stuffCategories')),
        costList: kvMap(grabBlock(b, 'costList')),
        workToBuild: statBases['WorkToBuild'] || null,
        power: grab(b, 'basePowerConsumption'),
        turret: !!(bld && grab(bld, 'turretGunDef')),
        recipes: liList(grabBlock(b, 'recipes')),
        research: liList(grabBlock(b, 'researchPrerequisites')),
        mineable: mineThing ? {
          thing: mineThing,
          yield_: parseFloat(grab(bld, 'mineableYield') || '0'),
          commonality: grab(bld, 'mineableScatterCommonality'),
          lump: grab(bld, 'mineableScatterLumpSizeRange'),
          hp: grab(grabBlock(b, 'statBases') || '', 'MaxHitPoints'),
        } : null,
      };
    } else if (kind === 'ResearchProjectDef') {
      research[def] = label || def;
    }
  }
}

// 配方 -> 建筑 反查
const recipeBuildings = {};
for (const [bDef, b] of Object.entries(buildings)) {
  for (const r of b.recipes) {
    (recipeBuildings[r] = recipeBuildings[r] || []).push(bDef);
  }
}

// ---------- 解析 HSK 详细文档（结构化） ----------
const lines = fs.readFileSync(HSK_DOC, 'utf8').split(/\r?\n/);
let cat = null, res = null;
const hskEntries = [];
const hskDefNames = new Set();
for (const line of lines) {
  let m = line.match(/^### \d+\.\d+ (.+?)\s*$/);
  if (m) { cat = m[1]; continue; }
  m = line.match(/^#### (.+?)（`([^`]+)`）/);
  if (m) { res = { label: m[1], def: m[2] }; hskDefNames.add(m[2]); continue; }
  if (line.startsWith('| ') && res) {
    const cells = line.trim().replace(/^\||\|$/g, '').split('|').map(s => s.trim());
    if (cells.length < 9 || cells[0] === '途径' || /^[-: ]+$/.test(cells[0])) continue;
    hskEntries.push({ cat, resDef: res.def, resLabel: res.label, cells });
  }
}
// 建筑/来源 defName 也计入合法集（用于别名验证）
for (const e of hskEntries) {
  const sm = e.cells[1].match(/`([^`]+)`/);
  if (sm) hskDefNames.add(sm[1]);
  const bm = e.cells[5].match(/建筑\s*[:：]\s*([^；]+)/);
  if (bm) bm[1].split(',').forEach(x => hskDefNames.add(x.trim()));
}

function mmtToHsk(mmtDef) {
  if (!mmtDef) return null;
  // MMT 部分配方直接产出/消耗原版 def（Steel、Silver、ComponentIndustrial 等）
  if (!mmtDef.startsWith('MMT_')) return hskDefNames.has(mmtDef) ? mmtDef : null;
  const x = mmtDef.slice(4);
  const cands = [MMT_ALIAS[x], x, x.endsWith('Ore') ? x.slice(0, -3) : null].filter(Boolean);
  for (const c of cands) if (hskDefNames.has(c)) return c;
  return null;
}

// HSK 输入/输出单元解析
function parseHskIns(cell) {
  if (!cell || cell === '—') return [];
  return cell.split('；').map(p => {
    p = p.trim();
    const qm = p.match(/^(.*?)\s*×\s*([\d.]+)$/);
    const body = qm ? qm[1].trim() : p;
    const qty = qm ? parseFloat(qm[2]) : null;
    const alts = body.split(' 或 ').map(a => {
      a = a.trim();
      const dm = a.match(/^([A-Za-z0-9_]+)（/);
      if (dm) return dm[1];
      const cm = a.match(/^\[(.+)\]$/);
      return cm ? `[${cm[1]}]` : a;
    });
    return { alts, qty };
  });
}
function parseHskOuts(cell) {
  if (!cell || cell === '—') return [];
  return cell.split('；').map(p => {
    const m = p.trim().match(/^([A-Za-z0-9_]+)（.*?）\s*×\s*(.+)$/);
    return m ? { def: m[1], qty: m[2] } : null;
  }).filter(Boolean);
}
function kvPairs(cell) {
  const out = {};
  if (!cell || cell === '—') return out;
  for (const part of cell.split('；')) {
    const m = part.trim().match(/^([^=]+)=(.+)$/);
    if (m) out[m[1].trim()] = m[2].trim();
  }
  return out;
}
function prePairs(cell) {
  const out = {};
  if (!cell || cell === '—') return out;
  for (const part of cell.split('；')) {
    const m = part.trim().match(/^(研究|技能|建筑)\s*[:：]\s*(.+)$/);
    if (m) out[m[1]] = m[2].trim();
  }
  return out;
}

const hskRecipes = [];  // 工作台配方
const hskVeins = [];    // 矿脉开采
for (const e of hskEntries) {
  const [way, srcCell, ins, outs, kv, pre] = e.cells;
  if (way === '工作台配方') {
    hskRecipes.push({
      def: (srcCell.match(/`([^`]+)`/) || [null, srcCell])[1],
      resDef: e.resDef, ins: parseHskIns(ins), outs: parseHskOuts(outs),
      kv: kvPairs(kv), pre: prePairs(pre),
    });
  } else if (way === '矿脉开采') {
    hskVeins.push({
      def: (srcCell.match(/`([^`]+)`/) || [null, srcCell])[1],
      srcLabel: srcCell.replace(/（`[^`]+`）/, '').trim(),
      resDef: e.resDef, outs: parseHskOuts(outs), kv: kvPairs(kv),
    });
  }
}

// ---------- 对比 ----------
// MMT 直接使用的原版/HSK def 的中文名（无 MMT_ 对应物时）
const ZH_DIRECT = {
  Steel: '钢', Silver: '银', Gold: '金', Plasteel: '铁钛合金',
  ComponentIndustrial: '工业零件', ComponentSpacer: '太空零件',
  ComponentAdvanced: '高级零件', WoodLog: '原木', Coal: '煤炭',
};
const zh = d => (labels[d] ? `${labels[d]}` : (ZH_DIRECT[d] ? `${ZH_DIRECT[d]}(${d})` : d)); // MMT 中文名
const zhHsk = d => {                                            // HSK def 用 MMT 对应中文
  for (const [m] of Object.entries(labels)) {
    if (mmtToHsk(m) === d) return labels[m];
  }
  return null;
};
function hskItemTxt(def, qty) {
  const z = zhHsk(def) || ZH_DIRECT[def];
  const name = z ? `${z}(${def})` : def;
  return qty != null ? `${name}×${qty}` : name;
}
function mmtIngTxt(ing) {
  const names = ing.defs.map(d => zh(d));
  const cats = ing.cats.map(c => `[${c}]`);
  return `${names.concat(cats).join(' 或 ')}×${ing.count}`;
}

function fmtNum(n) { return Number.isInteger(n) ? String(n) : String(n); }

// 为 MMT 配方找 HSK 对应配方
function matchHskRecipe(r) {
  const outHsk = r.prods.length ? mmtToHsk(r.prods[0].def) : null;
  if (!outHsk) return { cands: [], best: null, outHsk: null };
  const cands = hskRecipes.filter(h => h.outs.some(o => o.def === outHsk));
  const firstInHsk = r.ings.length && r.ings[0].defs.length ? mmtToHsk(r.ings[0].defs[0]) : null;
  const bldsHsk = (recipeBuildings[r.def] || []).map(mmtToHsk).filter(Boolean);
  let best = null, bestScore = -1;
  for (const h of cands) {
    let score = 0;
    if (firstInHsk && h.ins.length && h.ins[0].alts.includes(firstInHsk)) score += 2;
    const hBlds = (h.pre['建筑'] || '').split(',').map(s => s.trim());
    if (bldsHsk.some(b => hBlds.includes(b))) score += 1;
    if (score > bestScore) { bestScore = score; best = h; }
  }
  return { cands, best, outHsk };
}

function diffLine(r, h) {
  const diffs = [];
  // 首投入数量
  if (r.ings.length && h.ins.length) {
    const mIn = r.ings[0].count, hIn = h.ins[0].qty;
    if (hIn != null && mIn !== hIn) diffs.push(`首投入 ${fmtNum(mIn)} vs HSK ${fmtNum(hIn)}`);
  }
  // 产出数量
  const hOut = h.outs.find(o => o.def === mmtToHsk(r.prods[0].def));
  if (hOut && !isNaN(parseFloat(hOut.qty))) {
    const mOut = r.prods[0].qty, hQty = parseFloat(hOut.qty);
    if (mOut !== hQty) diffs.push(`产出 ${fmtNum(mOut)} vs HSK ${fmtNum(hQty)}`);
  }
  // 工作量
  const hWork = parseInt(h.kv['工作量'] || '', 10);
  if (!isNaN(hWork) && r.work !== hWork) diffs.push(`工作量 ${r.work} vs HSK ${hWork}`);
  // 技能
  const hSkill = h.pre['技能'] || '';
  const mSkill = r.skills.join(',');
  if (hSkill && mSkill && hSkill !== mSkill) diffs.push(`技能 ${mSkill} vs HSK ${hSkill}`);
  return diffs;
}

// ---------- 输出 ----------
const out = [];
out.push('# 当前 MOD（MMT）资源生产链路 与 HSK 对比');
out.push('');
out.push('- MMT 数据源：`1.7/Defs`（本 MOD）；HSK 数据源：[hsk-resource-production-paths.md](hsk-resource-production-paths.md)（`Hardcore-SK` 分支 `1.7-super-unstable`，提交 `b99b17d2`）。');
out.push('- 由 `tools/build_mmt_chains_compare.js` 自动生成；MMT 侧使用 MOD 实际中文 label，HSK 侧沿用 MMT 对应物的中文名并附 defName。');
out.push('- 匹配规则：产出物的 HSK 对应 def 相同，且首选投入/建筑别名一致者优先；`MMT 新增` 表示 HSK 无该资源或对应配方。');
out.push('- 矿脉说明：原版钢/银/金/玻璃钢矿脉已由 `1.7/Patches/MMT_Patch_VanillaVeins.xml` 改造为 MMT 铁矿/银矿/金矿/钛铁矿（HSK 做法），不再作为 MMT def 出现在下表；零件矿脉照 HSK 保留原版。');
out.push('- 第 5 节建筑造价的 HSK 侧直接解析 `Hardcore-SK/Mods/Core_SK/Defs/ThingDefs_Buildings`（沿 ParentName 链合并字段），不经 paths 文档。');
out.push('');

// 统计
let nSame = 0, nDiff = 0, nNew = 0, nNoMatch = 0;
const matchedHsk = new Set();

// 按 family 分组
const FAMILY_ZH = {
  Metal: '金属', Electric: '电动金属', Precious: '贵金属', AdvancedAlloys: '高级合金',
  Wood: '木材', ElectricWood: '电动木材', Concrete: '混凝土', Glass: '玻璃',
  Electronics: '电子', ElectricElectronics: '电动电子', Petrochem: '石化',
  ElectricPetrochem: '电动石化', Parts: '零件',
  Agriculture: '农业', Bionics: '仿生', Ceramics: '陶瓷', Drugs: '药品', Matter: '物质',
  Medicine: '医药', Nuclear: '核能', OrePowder: '矿粉', PetrochemExt: '石化扩展',
  ProcessorModules: '处理机模块', Robotics: '机器人', Shield: '护盾', Textiles: '纺织',
  TopMaterials: '顶级材料', TurretAmmo: '炮塔弹药', TurretCrates: '炮塔箱',
  WeaponParts: '武器部件', Weapons_Archaic: '古代武器', Weapons_Conventional: '常规武器',
  Weapons_Futuristic: '未来武器', ChargeShield: '充能护盾', PsychicShield: '灵能护盾',
  SieveMeshes: '筛网', StateBackup: '状态备份', BionicsRemove: '仿生移除',
};
const famOrder = [];
const byFam = new Map();
for (const r of recipes) {
  if (!byFam.has(r.family)) { byFam.set(r.family, []); famOrder.push(r.family); }
  byFam.get(r.family).push(r);
}

// 矿脉对比
const veins = Object.entries(buildings).filter(([, b]) => b.mineable);
out.push('## 1. 汇总');
out.push('');

// 先跑一遍配方统计
const recipeResults = [];
for (const r of recipes) {
  const { cands, best, outHsk } = matchHskRecipe(r);
  if (!outHsk) { nNew++; recipeResults.push({ r, kind: 'new' }); continue; }
  if (!best) { nNoMatch++; recipeResults.push({ r, kind: 'nomatch', outHsk, cands }); continue; }
  const diffs = diffLine(r, best);
  matchedHsk.add(best.def);
  if (diffs.length) { nDiff++; } else { nSame++; }
  recipeResults.push({ r, kind: 'matched', h: best, diffs, outHsk, cands });
}

out.push('| 项 | 数量 |');
out.push('| --- | --- |');
out.push(`| MMT 矿脉 | ${veins.length} |`);
out.push(`| MMT 配方 | ${recipes.length} |`);
out.push(`| 与 HSK 数值一致 | ${nSame} |`);
out.push(`| 与 HSK 有数值差异 | ${nDiff} |`);
out.push(`| HSK 有对应资源但无对应配方 | ${nNoMatch} |`);
out.push(`| MMT 新增（HSK 无对应资源） | ${nNew} |`);
out.push('');

// 按家族统计
out.push('### 1.1 按家族统计');
out.push('');
out.push('| 家族 | 配方数 | 一致 | 差异 | 无对应配方 | MMT 新增 |');
out.push('| --- | --- | --- | --- | --- | --- |');
for (const fam of famOrder) {
  const fr = recipeResults.filter(x => x.r.family === fam);
  if (!fr.length) continue;
  const c = k => fr.filter(x => x.kind === k).length;
  const nSameFam = fr.filter(x => x.kind === 'matched' && !x.diffs.length).length;
  const nDiffFam = fr.filter(x => x.kind === 'matched' && x.diffs.length).length;
  out.push(`| ${FAMILY_ZH[fam] || fam}（${fam}） | ${fr.length} | ${nSameFam} | ${nDiffFam} | ${c('nomatch')} | ${c('new')} |`);
}
out.push('');

// 矿脉章节
out.push('## 2. 矿脉对比');
out.push('');
out.push('| MMT 矿脉 | 产出 | MMT 单格产量 | MMT 矿团/散布/耐久 | HSK 对应矿脉 | HSK 单格产量 | HSK 矿团/散布/耐久 | 对比 |');
out.push('| --- | --- | --- | --- | --- | --- | --- | --- |');
for (const [vDef, b] of veins) {
  const mn = b.mineable;
  const hskThing = mmtToHsk(mn.thing);
  // 优先匹配真正的矿脉（带矿团规模），其次才是陨石等无散布参数的来源
  const hv = hskThing
    ? (hskVeins.find(v => v.resDef === hskThing && v.kv['矿团规模'])
      || hskVeins.find(v => v.resDef === hskThing && v.kv['单格产量']))
    : null;
  const hYield = hv ? parseFloat(hv.kv['单格产量']) : null;
  let verdict;
  if (!hskThing) verdict = 'MMT 新增资源';
  else if (!hv) verdict = 'HSK 无矿脉（仅深钻等）';
  else verdict = (mn.yield_ === hYield) ? '产量一致' : `产量 ${fmtNum(mn.yield_)} vs HSK ${fmtNum(hYield)}`;
  const mInfo = `${mn.lump || '—'} / ${mn.commonality || '—'} / ${mn.hp || '—'}`;
  const hInfo = hv ? `${hv.kv['矿团规模'] || '—'} / ${hv.kv['散布常见度'] || '—'} / ${hv.kv['耐久'] || '—'}` : '—';
  out.push(`| ${zh(vDef)}(${vDef}) | ${zh(mn.thing)} | ${fmtNum(mn.yield_)} | ${mInfo} | ${hv ? `${hv.srcLabel}(\`${hv.def}\`)` : '—'} | ${hYield != null ? fmtNum(hYield) : '—'} | ${hInfo} | ${verdict} |`);
}
out.push('');

// 配方章节
out.push('## 3. 配方对比（按 MMT 家族分组）');
out.push('');
let sec = 0;
for (const fam of famOrder) {
  const famRecipes = recipeResults.filter(x => x.r.family === fam);
  if (!famRecipes.length) continue;
  sec++;
  out.push(`### 3.${sec} ${FAMILY_ZH[fam] || fam}（${fam}）`);
  out.push('');
  for (const x of famRecipes) {
    const r = x.r;
    const blds = (recipeBuildings[r.def] || []).map(b => zh(b)).join(', ') || '—';
    const inTxt = r.ings.map(mmtIngTxt).join(' + ') || '—';
    const outTxt = r.prods.map(p => `${zh(p.def)}×${fmtNum(p.qty)}`).join(' + ') || '—';
    const resTxt = r.research ? `研究:${research[r.research] || r.research}(${r.research})` : '';
    const skTxt = r.skills.length ? `技能:${r.skills.join(',')}` : '';
    const pre = [resTxt, skTxt].filter(Boolean).join('，');
    out.push(`#### ${r.label}（\`${r.def}\`）`);
    out.push('');
    out.push(`- **MMT**：${inTxt} -> ${outTxt} @ ${blds} ｜ 工作量=${r.work}${pre ? ' ｜ ' + pre : ''}`);
    if (x.kind === 'new') {
      out.push('- **HSK**：无对应资源（MMT 新增）');
    } else if (x.kind === 'nomatch') {
      out.push(`- **HSK**：有资源 ${x.outHsk}，但无投入匹配的配方（HSK 侧共有 ${x.cands.length} 条产出配方）`);
    } else {
      const h = x.h;
      const hIn = h.ins.map(g => `${g.alts.map(a => hskItemTxt(a, null)).join(' 或 ')}${g.qty != null ? '×' + g.qty : ''}`).join(' + ') || '—';
      const hOut = h.outs.map(o => hskItemTxt(o.def, o.qty)).join(' + ');
      const hPre = [];
      if (h.pre['研究']) hPre.push(`研究:${h.pre['研究']}`);
      if (h.pre['技能']) hPre.push(`技能:${h.pre['技能']}`);
      const hBld = h.pre['建筑'] ? ` @ ${h.pre['建筑'].split(',').map(b => { const bn = b.trim(); return hskItemTxt(bn, null); }).join(', ')}` : '';
      out.push(`- **HSK** \`${h.def}\`：${hIn} -> ${hOut}${hBld} ｜ 工作量=${h.kv['工作量'] || '—'}${hPre.length ? ' ｜ ' + hPre.join('，') : ''}`);
      out.push(`- **对比**：${x.diffs.length ? x.diffs.join('；') : '数值一致'}`);
    }
    out.push('');
  }
}

// HSK 侧未覆盖
const famHskDefs = new Set();
for (const r of recipes) for (const p of r.prods) {
  const h = mmtToHsk(p.def);
  if (h) famHskDefs.add(h);
}
const uncovered = hskRecipes.filter(h => famHskDefs.has(h.resDef) && !matchedHsk.has(h.def));
out.push('## 4. HSK 有而 MMT 未实现的配方（限于 MMT 涉及的资源）');
out.push('');
out.push('以下 HSK 配方产出 MMT 已涉及的资源，但 MMT 尚未实现对应配方，可作为后续补全清单。');
out.push('');
if (!uncovered.length) {
  out.push('- 无。');
} else {
  const byRes = new Map();
  for (const h of uncovered) {
    if (!byRes.has(h.resDef)) byRes.set(h.resDef, []);
    byRes.get(h.resDef).push(h);
  }
  let resSec = 0;
  for (const [resDef, hs] of byRes) {
    resSec++;
    out.push(`### 4.${resSec} ${hskItemTxt(resDef, null)}`);
    out.push('');
    for (const h of hs) {
      const hIn = h.ins.map(g => `${g.alts.map(a => hskItemTxt(a, null)).join(' 或 ')}${g.qty != null ? '×' + g.qty : ''}`).join(' + ') || '—';
      const hOut = h.outs.map(o => hskItemTxt(o.def, o.qty)).join(' + ') || '—';
      const hBld = h.pre['建筑'] ? ` @ ${h.pre['建筑'].split(',').map(b => hskItemTxt(b.trim(), null)).join(', ')}` : '';
      const hPre = [];
      if (h.pre['研究']) hPre.push(`研究:${h.pre['研究']}`);
      if (h.pre['技能']) hPre.push(`技能:${h.pre['技能']}`);
      out.push(`- \`${h.def}\`：${hIn} -> ${hOut}${hBld} ｜ 工作量=${h.kv['工作量'] || '—'}${hPre.length ? ' ｜ ' + hPre.join('，') : ''} ｜ **MMT 未实现**`);
    }
    out.push('');
  }
}
out.push('');

// ---------- 5. 建筑造价对比 ----------
// HSK 建筑直接解析 Core_SK XML（带 ParentName 父类回填）
const HSK_BLD_DIR = path.join(ROOT, 'Hardcore-SK', 'Mods', 'Core_SK', 'Defs', 'ThingDefs_Buildings');
const hskRaw = {}; // defName 或 Name(抽象基类) -> 原始字段
for (const f of walk(HSK_BLD_DIR)) {
  const t = fs.readFileSync(f, 'utf8');
  for (const m of t.matchAll(/<ThingDef\b([^>]*)>([\s\S]*?)<\/ThingDef>/g)) {
    const attrs = m[1], b = m[2];
    const def = grab(b, 'defName');
    const key = def || (attrs.match(/Name="([^"]+)"/) || [])[1];
    if (!key) continue;
    hskRaw[key] = {
      def, file: path.basename(f),
      parent: (attrs.match(/ParentName="([^"]+)"/) || [])[1] || null,
      label: grab(b, 'label'),
      size: grab(b, 'size'),
      costStuffCount: grab(b, 'costStuffCount'),
      stuffCats: liList(grabBlock(b, 'stuffCategories')),
      costList: kvMap(grabBlock(b, 'costList')),
      workToBuild: kvMap(grabBlock(b, 'statBases'))['WorkToBuild'] || null,
      power: grab(b, 'basePowerConsumption'),
      recipes: liList(grabBlock(b, 'recipes')),
    };
  }
}
function hskMerged(key) {
  const chain = [];
  let cur = key, guard = 0;
  while (cur && hskRaw[cur] && guard++ < 20) { chain.push(hskRaw[cur]); cur = hskRaw[cur].parent; }
  if (!chain.length) return null;
  const merged = { def: chain[0].def, file: chain[0].file, costList: {} };
  for (const f of ['label', 'size', 'costStuffCount', 'workToBuild', 'power']) {
    for (const r of chain) if (r[f] != null) { merged[f] = r[f]; break; }
    if (merged[f] === undefined) merged[f] = null;
  }
  merged.stuffCats = [];
  for (const r of chain) if (r.stuffCats && r.stuffCats.length) { merged.stuffCats = r.stuffCats; break; }
  for (const r of [...chain].reverse()) Object.assign(merged.costList, r.costList);
  return merged;
}
function hskBuildingOf(mmtDef) {
  const x = mmtDef.startsWith('MMT_') ? mmtDef.slice(4) : mmtDef;
  for (const c of [MMT_ALIAS[x], x].filter(Boolean)) {
    if (hskRaw[c] && hskRaw[c].def) return c;
  }
  return null;
}
const STUFF_CAT_ZH = { Metallic: '金属', Woody: '木质', Stony: '石质', Leathery: '皮革', Fabric: '织物' };
function costTxt(costList, costStuffCount, stuffCats, nameFn) {
  const parts = Object.entries(costList).map(([d, n]) => `${nameFn(d)}×${n}`);
  if (costStuffCount) parts.push(`[${(stuffCats || []).map(c => STUFF_CAT_ZH[c] || c).join('/') || '材料'}]×${costStuffCount}`);
  return parts.join(' + ') || '—';
}

const bldRows = Object.entries(buildings)
  .filter(([, b]) => b.dir === 'ThingDefs_Buildings' && !b.mineable && !b.turret)
  .sort((a, b2) => (a[1].file + '/' + a[0]).localeCompare(b2[1].file + '/' + b2[0]));
let nBldSame = 0, nBldDiff = 0, nBldNew = 0;
out.push('## 5. 建筑造价对比');
out.push('');
out.push('- 跳过矿脉（见第 2 节）与炮塔（属武器范畴，见 [mmt-weapons.md](mmt-weapons.md)）；`[类别]×n` 为 stuff 材料（costStuffCount）；功耗/工作量列格式 `功率W / WorkToBuild`。');
out.push('- MMT 建筑普遍继承原版 `BuildingBase`（造价/尺寸都在子类上，无需父类回填）；HSK 侧已沿 ParentName 链合并。');
out.push('');
out.push('| MMT 建筑 | 尺寸 | MMT 造价 | MMT 功耗/工作量 | HSK 对应 | HSK 造价 | HSK 功耗/工作量 | 对比 |');
out.push('| --- | --- | --- | --- | --- | --- | --- | --- |');
for (const [bDef, b] of bldRows) {
  const hKey = hskBuildingOf(bDef);
  const h = hKey ? hskMerged(hKey) : null;
  const mCost = costTxt(b.costList, b.costStuffCount, b.stuffCats, d => zh(d));
  const mPw = [b.power ? `${b.power}W` : null, b.workToBuild].filter(Boolean).join(' / ') || '—';
  let hName = '—', hCost = '—', hPw = '—', verdict = 'MMT 新增';
  if (h) {
    hName = `${h.label || hKey}(\`${hKey}\`)`;
    hCost = costTxt(h.costList, h.costStuffCount, h.stuffCats, d => hskItemTxt(d, null));
    hPw = [h.power ? `${h.power}W` : null, h.workToBuild].filter(Boolean).join(' / ') || '—';
    const diffs = [];
    if (String(b.costStuffCount || '') !== String(h.costStuffCount || '')) diffs.push(`材料量 ${b.costStuffCount || '—'} vs ${h.costStuffCount || '—'}`);
    const usedHsk = new Set();
    for (const [d, n] of Object.entries(b.costList)) {
      const hk = mmtToHsk(d) || d;
      if (h.costList[hk] != null) {
        usedHsk.add(hk);
        if (String(h.costList[hk]) !== String(n)) diffs.push(`${zh(d)} ${n} vs ${h.costList[hk]}`);
      } else diffs.push(`MMT 多 ${zh(d)}×${n}`);
    }
    for (const [d, n] of Object.entries(h.costList)) {
      if (!usedHsk.has(d)) diffs.push(`HSK 多 ${hskItemTxt(d, null)}×${n}`);
    }
    if (String(b.power || '') !== String(h.power || '')) diffs.push(`功耗 ${b.power || '—'} vs ${h.power || '—'}`);
    if (String(b.workToBuild || '') !== String(h.workToBuild || '')) diffs.push(`工作量 ${b.workToBuild || '—'} vs ${h.workToBuild || '—'}`);
    verdict = diffs.length ? diffs.join('；') : '一致';
    if (diffs.length) nBldDiff++; else nBldSame++;
  } else nBldNew++;
  out.push(`| ${zh(bDef)}(\`${bDef}\`) | ${b.size || '—'} | ${mCost} | ${mPw} | ${hName} | ${hCost} | ${hPw} | ${verdict} |`);
}
out.push('');
out.push(`建筑对比小结：一致 ${nBldSame} / 差异 ${nBldDiff} / MMT 新增 ${nBldNew}（共 ${bldRows.length}）。`);
out.push('');

fs.writeFileSync(DST, out.join('\n') + '\n', 'utf8');
console.log(DST);
console.log(`MMT 矿脉 ${veins.length}，配方 ${recipes.length}：一致 ${nSame} / 差异 ${nDiff} / 无对应配方 ${nNoMatch} / 新增 ${nNew}`);
console.log(`HSK 侧未覆盖配方 ${uncovered.length} 条`);
console.log(`建筑对比：一致 ${nBldSame} / 差异 ${nBldDiff} / MMT 新增 ${nBldNew}`);
