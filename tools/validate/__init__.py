# 新链校验包：每条新资源链一个模块（如 nuclear.py），
# 模块内定义 validate_* 函数并在末尾注册 SUITES 字典，
# 由 tools/validate_new.py 自动发现聚合并提供 CLI。
# 共享 helper 直接 from validate_mod import ...（旧文件不改）。
