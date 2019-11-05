﻿using UnityEngine;
using AGXUnity.Utils;

namespace AGXUnity
{
  [AddComponentMenu( "AGXUnity/Deformable Terrain Shovel" )]
  [RequireComponent( typeof( RigidBody ) )]
  public class DeformableTerrainShovel : ScriptComponent
  {
    /// <summary>
    /// Native instance of this shovel.
    /// </summary>
    [HideInInspector]
    public agxTerrain.Shovel Native { get; private set; } = null;

    /// <summary>
    /// Rigid body component of this shovel.
    /// </summary>
    [HideInInspector]
    public RigidBody RigidBody { get { return m_rb ?? ( m_rb = GetComponent<RigidBody>() ); } }

    [SerializeField]
    private Line m_topEdge = null;

    public Line TopEdge
    {
      get
      {
        if ( m_topEdge == null || !m_topEdge.Valid )
          m_topEdge = Line.Create( RigidBody.gameObject,
                                   Vector3.left + Vector3.up,
                                   Vector3.right + Vector3.up );
        return m_topEdge;
      }
      set
      {
        m_topEdge = value ?? Line.Create( RigidBody.gameObject,
                                          Vector3.left + Vector3.up,
                                          Vector3.right + Vector3.up );
        if ( Native != null )
          Native.setTopEdge( m_topEdge.ToNativeEdge( RigidBody.gameObject ) );
      }
    }

    [SerializeField]
    private Line m_cuttingEdge = null;

    public Line CuttingEdge
    {
      get
      {
        if ( m_cuttingEdge == null || !m_cuttingEdge.Valid )
          m_cuttingEdge = Line.Create( RigidBody.gameObject,
                                       Vector3.left + Vector3.down,
                                       Vector3.right + Vector3.down );
        return m_cuttingEdge;
      }
      set
      {
        m_cuttingEdge = value ?? Line.Create( RigidBody.gameObject,
                                              Vector3.left + Vector3.down,
                                              Vector3.right + Vector3.down );
        if ( Native != null )
          Native.setCuttingEdge( m_cuttingEdge.ToNativeEdge( RigidBody.gameObject ) );
      }
    }

    [SerializeField]
    private Line m_cuttingDirection = null;

    public Line CuttingDirection
    {
      get
      {
        if ( m_cuttingDirection == null || !m_cuttingDirection.Valid )
          m_cuttingDirection = Line.Create( RigidBody.gameObject,
                                            Vector3.down,
                                            Vector3.down + Vector3.forward );
        return m_cuttingDirection;
      }
      set
      {
        m_cuttingDirection = value ?? Line.Create( RigidBody.gameObject,
                                                   Vector3.down,
                                                   Vector3.down + Vector3.forward );
        if ( Native != null )
          Native.setCuttingDirection( m_cuttingDirection.CalculateLocalDirection( RigidBody.gameObject ).ToHandedVec3() );
      }
    }

    [SerializeField]
    private DeformableTerrainShovelSettings m_settings = null;

    [AllowRecursiveEditing]
    public DeformableTerrainShovelSettings Settings
    {
      get { return m_settings; }
      set
      {
        if ( Native != null && m_settings != null && m_settings != value )
          m_settings.Unregister( this );

        m_settings = value;

        if ( Native != null && m_settings != null )
          m_settings.Register( this );
      }
    }

    protected override bool Initialize()
    {
      var rb = RigidBody?.GetInitialized<RigidBody>()?.Native;
      if ( rb == null )
        return false;

      Native = new agxTerrain.Shovel( rb,
                                      TopEdge.ToNativeEdge( gameObject ),
                                      CuttingEdge.ToNativeEdge( gameObject ),
                                      CuttingDirection.CalculateLocalDirection( gameObject ).ToHandedVec3() );

      if ( Settings == null ) {
        Settings = ScriptAsset.Create<DeformableTerrainShovelSettings>();
        Settings.name = "[Temporary]Shovel Settings";
      }

      return true;
    }

    protected override void OnDestroy()
    {
      if ( Settings != null )
        Settings.Unregister( this );

      Native = null;

      base.OnDestroy();
    }

    protected override void OnEnable()
    {
      if ( Native != null )
        Native.setEnable( true );
    }

    protected override void OnDisable()
    {
      if ( Native != null )
        Native.setEnable( false );
    }

    private RigidBody m_rb = null;
  }
}
